using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private readonly ILogger<BidPlacedConsumer> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;

    public BidPlacedConsumer(IHubContext<NotificationHub> hubContext, ILogger<BidPlacedConsumer> logger)
    {
        _logger = logger;
        _hubContext = hubContext;
    }
    
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        _logger.LogInformation("Bid placed message received");
        await _hubContext.Clients.All.SendAsync("BidPlaced", context.Message);
    }
}