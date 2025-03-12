using AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var (auctionsDb, identityDb) = DatabaseBuilders.BuildPostgres(builder);
var mongodb = DatabaseBuilders.BuildMongodb(builder);
//amqp://guest:guest@localhost:64858
var rabbitPw = builder.AddParameter("rabbitmq-password", false);
#pragma warning disable ASPIREPROXYENDPOINTS001
var rabbitmq = builder.AddRabbitMQ("messaging", password: rabbitPw)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpointProxySupport(false)
    .WithManagementPlugin(port: 15672);
#pragma warning restore ASPIREPROXYENDPOINTS001

var auctionService = builder.AddProject<Projects.AuctionService>("auctionservice");
var searchService = builder.AddProject<Projects.SearchService>("searchservice");
var identityService = builder.AddProject<Projects.IdentityService>("identityservice")
    .WithReference(identityDb)
    .WaitFor(identityDb);
var gatewayService = builder.AddProject<Projects.GatewayService>("gatewayservice");


var webAppFrontend = builder
    .AddNpmApp("frontend", "../../frontend/web-app", "dev")
    .WithNpmPackageInstallation()
    .WithHttpEndpoint(port: 3000, isProxied: false) 
    .WithExternalHttpEndpoints()
    .WithReference(gatewayService);

auctionService
    .WithReference(auctionsDb)
    .WithReference(rabbitmq)
    .WithReference(identityService)
    .WaitFor(auctionsDb)
    .WaitFor(rabbitmq);

searchService
    .WithReference(mongodb)
    .WithReference(rabbitmq)
    .WaitFor(mongodb)
    .WaitFor(rabbitmq)
    .WithReference(auctionService);

gatewayService
    .WithReference(auctionService)
    .WithReference(searchService)
    .WithReference(identityService);


builder.Build().Run();
