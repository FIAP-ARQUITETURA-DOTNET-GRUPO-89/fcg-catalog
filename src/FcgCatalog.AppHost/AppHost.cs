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

//var mongodb = isTesting
//    ? builder.AddMongoDB("mongodb")
//        .WithLifetime(ContainerLifetime.Session)
//        .AddDatabase("fcg_catalog_db")
//    : builder.AddMongoDB("mongodb", port: 27017)
//        .WithLifetime(ContainerLifetime.Persistent)
//        .AddDatabase("fcg_catalog_db");

builder.AddProject<Projects.FcgCatalog_Api>("fcgcatalog-api")
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
        .WithReference(postgres)
        .WithReference(rabbitmq)
        //.WithReference(mongodb)
        .WaitFor(postgres)
        .WaitFor(rabbitmq);
//.WaitFor(mongodb);

builder.AddProject<Projects.FcgCatalog_Worker>("fcgcatalog-worker")
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
        .WithReference(postgres)
        .WithReference(rabbitmq)
        //.WithReference(mongodb)
        .WaitFor(postgres)
        .WaitFor(rabbitmq);
        //.WaitFor(mongodb);

builder.Build().Run();
