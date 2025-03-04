var builder = DistributedApplication.CreateBuilder(args);

#pragma warning disable ASPIREPROXYENDPOINTS001
var postgres = builder.AddPostgres("postgres", port: 5432)
    .WithPgWeb()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
    //.WithEndpointProxySupport(false); KEEP RUNNING AND BEING CONNECTABLE EVEN WITHOUT ASPIRE
#pragma warning restore ASPIREPROXYENDPOINTS001

var auctionsDb = postgres.AddDatabase("auctions");

var auctionService = builder.AddProject<Projects.AuctionService>("auctionservice")
    .WithReference(auctionsDb)
    .WaitFor(auctionsDb);

builder.Build().Run();
