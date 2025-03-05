using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Routes;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddHttpClient<AuctionSvcHttpClient>(client =>
{
    client.BaseAddress = new("http://auctionservice");
})
.AddPolicyHandler(GetPolicy());

var app = builder.Build();


app.UseHttpsRedirection();

app.MapSearchRoutes();

app.Lifetime.ApplicationStarted.Register(async void () =>
{
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
});


app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy() 
    => HttpPolicyExtensions.HandleTransientHttpError()
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));
