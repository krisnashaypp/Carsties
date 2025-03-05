using AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var auctionsDb = DatabaseBuilders.BuildPostgres(builder);
var mongodb = DatabaseBuilders.BuildMongodb(builder);

var auctionService = builder.AddProject<Projects.AuctionService>("auctionservice")
    .WithReference(auctionsDb)
    .WaitFor(auctionsDb);

var searchService = builder.AddProject<Projects.SearchService>("searchservice")
    .WithReference(mongodb)
    .WaitFor(mongodb)
    .WithReference(auctionService);


builder.Build().Run();
