using AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var (auctionsDb, identityDb) = DatabaseBuilders.BuildPostgres(builder);
var mongodb = DatabaseBuilders.BuildMongodb(builder);

var rabbitPw = builder.AddParameter("rabbitmq-password", false);
var rabbitmq = builder.AddRabbitMQ("messaging", password: rabbitPw)
    .WithDataVolume()
    .WithManagementPlugin(port: 15672);

var auctionService = builder.AddProject<Projects.AuctionService>("auctionservice");
var searchService = builder.AddProject<Projects.SearchService>("searchservice");
var identityService = builder.AddProject<Projects.IdentityService>("identityservice")
    .WithReference(identityDb)
    .WaitFor(identityDb);
var gatewayService = builder.AddProject<Projects.GatewayService>("gatewayservice");

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
