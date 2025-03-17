using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    private readonly ILogger<AuctionFinishedConsumer> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;

    public AuctionFinishedConsumer(IHubContext<NotificationHub> hubContext, ILogger<AuctionFinishedConsumer> logger)
    {
        _logger = logger;
        _hubContext = hubContext;
    }
    
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        _logger.LogInformation("Auction finished message received");
        await _hubContext.Clients.All.SendAsync("AuctionFinished", context.Message);
    }
}