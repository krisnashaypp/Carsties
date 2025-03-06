using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.OpenTelemetry()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {MessageType} {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Literate,
        applyThemeToRedirectedOutput:true)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Services.AddSerilog(Log.Logger);

builder.Services.AddControllers();
builder.AddServiceDefaults();
builder.Services.AddDbContext<AuctionDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("auctions"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<AuctionDbContext>(options =>
    {
        options.QueryDelay = TimeSpan.FromSeconds(10);
        options.UsePostgres();
        options.UseBusOutbox();
    });
    
    x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
    
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(builder.Configuration.GetConnectionString("messaging")));
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}

app.Run();
