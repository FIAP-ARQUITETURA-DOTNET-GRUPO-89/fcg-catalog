using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var isTesting = builder.Environment.IsEnvironment("Testing");

var postgres = isTesting
    ? builder.AddPostgres("Postgres")
        .WithLifetime(ContainerLifetime.Session)
        .AddDatabase("Default", "fcgcatalog-db")
    : builder.AddPostgres("Postgres", port: 5432)
        .WithLifetime(ContainerLifetime.Persistent)
        .WithPgAdmin(c => c.WithLifetime(ContainerLifetime.Persistent))
        .AddDatabase("Default", "fcgcatalog-db");

var rabbitmq = isTesting
    ? builder.AddRabbitMQ("rabbitmq")
        .WithManagementPlugin()
        .WithLifetime(ContainerLifetime.Session)
    : builder.AddRabbitMQ("rabbitmq")
        .WithManagementPlugin()
        .WithLifetime(ContainerLifetime.Persistent);

var redis = builder.AddRedis("redis");

builder.AddProject<Projects.FcgCatalog_Api>("fcgcatalog-api")
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
        .WithReference(postgres)
        .WithReference(rabbitmq)
        .WithReference(redis)
        .WaitFor(postgres)
        .WaitFor(rabbitmq)
        .WaitFor(redis); ;

builder.AddProject<Projects.FcgCatalog_Worker>("fcgcatalog-worker")
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
        .WithReference(postgres)
        .WithReference(rabbitmq)
        .WaitFor(postgres)
        .WaitFor(rabbitmq);

builder.Build().Run();
