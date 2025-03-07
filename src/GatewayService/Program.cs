using Microsoft.AspNetCore.Authentication.JwtBearer;
using ServiceDefaults;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var routes = new[]
{
    new RouteConfig()
    {
        RouteId = "auctionsRead",
        ClusterId = "auctions",
        Match = new RouteMatch
        {
            Path = "/auctions/{**catch-all}",
            Methods = ["GET"]
        },
        Transforms = new List<Dictionary<string, string>>()
        {
            new()
            {
                { "PathPattern", "api/auctions/{**catch-all}" }
            }
        }
    },
    new RouteConfig()
    {
        RouteId = "auctionsWrite",
        AuthorizationPolicy = "default",
        ClusterId = "auctions",
        Match = new RouteMatch
        {
            Path = "/auctions/{**catch-all}",
            Methods = ["POST", "PUT", "DELETE"]
        },
        Transforms = new List<Dictionary<string, string>>()
        {
            new()
            {
                { "PathPattern", "api/auctions/{**catch-all}" }
            }
        }
    },
    new RouteConfig()
    {
        RouteId = "search",
        ClusterId = "search",
        Match = new RouteMatch
        {
            Path = "/search/{**catch-all}",
            Methods = ["GET"]
        },
        Transforms = new List<Dictionary<string, string>>()
        {
            new()
            {
                { "PathPattern", "api/search/{**catch-all}" }
            }
        }
    }
};

var clusters = new[]
{
    new ClusterConfig()
    {
        ClusterId = "auctions",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            {
                "auctionsApi", new DestinationConfig()
                {
                    Address = builder.GetServiceLocation(ServiceType.AUCTION)
                }
            }
        }
    },
    new ClusterConfig()
    {
        ClusterId = "search",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            {
                "searchApi", new DestinationConfig()
                {
                    Address = builder.GetServiceLocation(ServiceType.SEARCH)
                }
            }
        }
    }
};

builder.Services.AddReverseProxy()
    .LoadFromMemory(routes, clusters);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var duendeUrl = builder.GetServiceLocation(ServiceType.IDENTITY);
        
        options.Authority = duendeUrl;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";
        options.TokenValidationParameters.ValidIssuers = [duendeUrl];
    });

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapReverseProxy();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
