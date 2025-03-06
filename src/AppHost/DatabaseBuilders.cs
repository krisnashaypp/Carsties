using Microsoft.Extensions.Configuration;

namespace AppHost;

public class DatabaseBuilders
{
    public static (IResourceBuilder<PostgresDatabaseResource> AuctionsDb, IResourceBuilder<PostgresDatabaseResource> IdentityDb) BuildPostgres(IDistributedApplicationBuilder distributedApplicationBuilder)
    {
#pragma warning disable ASPIREPROXYENDPOINTS001
        var postgres = distributedApplicationBuilder.AddPostgres("postgres", port: 5432)
            .WithPgWeb()
            .WithDataVolume()
            .WithLifetime(ContainerLifetime.Persistent);
        //.WithEndpointProxySupport(false); KEEP RUNNING AND BEING CONNECTABLE EVEN WITHOUT ASPIRE
#pragma warning restore ASPIREPROXYENDPOINTS001

        var auctionsDb = postgres.AddDatabase("auctions");
        var identityDb = postgres.AddDatabase("identity");
        return (auctionsDb, identityDb);
    }
    
    
    
    public static IResourceBuilder<IResourceWithConnectionString> BuildMongodb(IDistributedApplicationBuilder mongoBuilder)
    {
        if (!string.IsNullOrWhiteSpace(mongoBuilder.Configuration.GetConnectionString("MongoDbConnection")))
        {
            Console.WriteLine("Using existing mongodb connection");
            return mongoBuilder.AddConnectionString("MongoDbConnection");
        }
        Console.WriteLine("Creating new mongodb connection");
        var mongo = mongoBuilder.AddMongoDB("mongo")
            .WithLifetime(ContainerLifetime.Persistent);

        var mongodb = mongo.AddDatabase("MongoDbConnection");
        return mongodb;
    }
}