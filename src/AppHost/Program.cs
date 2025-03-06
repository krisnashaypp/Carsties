using AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var (auctionsDb, identityDb) = DatabaseBuilders.BuildPostgres(builder);
var mongodb = DatabaseBuilders.BuildMongodb(builder);

var rabbitPw = builder.AddParameter("rabbitmq-password", false);
var rabbitmq = builder.AddRabbitMQ("messaging", password: rabbitPw)
    .WithDataVolume()
    .WithManagementPlugin(port: 15672);

var auctionService = builder.AddProject<Projects.AuctionService>("auctionservice")
    .WithReference(auctionsDb)
    .WithReference(rabbitmq)
    .WaitFor(auctionsDb)
    .WaitFor(rabbitmq);

var searchService = builder.AddProject<Projects.SearchService>("searchservice")
    .WithReference(mongodb)
    .WithReference(rabbitmq)
    .WaitFor(mongodb)
    .WaitFor(rabbitmq)
    .WithReference(auctionService);

// var identityService = builder.AddProject<Projects.IdentityService>("identityservice")
//     .WithReference(identityDb)
//     .WaitFor(identityDb);


builder.Build().Run();
