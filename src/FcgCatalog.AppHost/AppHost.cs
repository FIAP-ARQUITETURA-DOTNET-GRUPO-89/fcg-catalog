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

var mongodb = isTesting
    ? builder.AddMongoDB("mongodb")
        .WithLifetime(ContainerLifetime.Session)
        .AddDatabase("Mongo")
    : builder.AddMongoDB("mongodb", port: 27017)
        .WithLifetime(ContainerLifetime.Persistent)
        .AddDatabase("Mongo");
var redis = builder.AddRedis("redis");

var opensearch = builder.AddContainer("opensearch", "opensearchproject/opensearch", "2.19.1")
    .WithEnvironment("discovery.type", "single-node")
    .WithEnvironment("DISABLE_SECURITY_PLUGIN", "true")
    .WithEnvironment("DISABLE_INSTALL_DEMO_CONFIG", "true")
    .WithEnvironment("OPENSEARCH_JAVA_OPTS", "-Xms512m -Xmx512m")
    .WithHttpEndpoint(port: isTesting ? null : 9200, targetPort: 9200, name: "http")
    .WithHttpHealthCheck("/_cluster/health", endpointName: "http")
    .WithLifetime(isTesting ? ContainerLifetime.Session : ContainerLifetime.Persistent);

builder.AddProject<Projects.FcgCatalog_Api>("fcgcatalog-api")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithReference(postgres)
    .WithReference(rabbitmq)
    .WithReference(mongodb)
    .WithReference(redis)
    .WithEnvironment("OpenSearch__Url", opensearch.GetEndpoint("http"))
    .WaitFor(postgres)
    .WaitFor(rabbitmq)
    .WaitFor(mongodb)
    .WaitFor(redis)
    .WaitFor(opensearch);

builder.AddProject<Projects.FcgCatalog_Worker>("fcgcatalog-worker")
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
        .WithReference(postgres)
        .WithReference(rabbitmq)
        .WithReference(mongodb)
        .WaitFor(postgres)
        .WaitFor(rabbitmq)
        .WaitFor(mongodb);

builder.Build().Run();
