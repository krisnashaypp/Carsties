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
    },
    new RouteConfig()
    {
        RouteId = "bidsWrite",
        ClusterId = "bids",
        AuthorizationPolicy = "default",
        Match = new RouteMatch
        {
            Path = "/bids",
            Methods = ["POST"]
        },
        Transforms = new List<Dictionary<string, string>>()
        {
            new()
            {
                { "PathPattern", "api/bids" }
            }
        }
    },
    new RouteConfig()
    {
        RouteId = "bidsRead",
        ClusterId = "bids",
        Match = new RouteMatch
        {
            Path = "/bids/{**catch-all}",
            Methods = ["GET"]
        },
        Transforms = new List<Dictionary<string, string>>()
        {
            new()
            {
                { "PathPattern", "api/bids/{**catch-all}" }
            }
        }
    },
    new RouteConfig()
    {
        RouteId = "notification",
        ClusterId = "notification",
        CorsPolicy = "customPolicy",
        Match = new RouteMatch
        {
            Path = "/notifications/{**catch-all}"
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
    },
    new ClusterConfig()
    {
        ClusterId = "bids",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            {
                "bidsApi", new DestinationConfig()
                {
                    Address = builder.GetServiceLocation(ServiceType.BIDS)
                }
            }
        }
    },
    new ClusterConfig()
    {
        ClusterId = "notification",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            {
                "notifyApi", new DestinationConfig()
                {
                    Address = builder.GetServiceLocation(ServiceType.NOTIFICATION)
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("customPolicy", b =>
    {
        b.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins(builder.GetServiceLocation(ServiceType.FRONTEND));
    });
});

var app = builder.Build();

app.UseCors();

app.MapReverseProxy();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
