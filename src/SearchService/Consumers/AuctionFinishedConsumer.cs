using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    private readonly ILogger<AuctionFinishedConsumer> _logger;

    public AuctionFinishedConsumer(ILogger<AuctionFinishedConsumer> logger)
    {
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        _logger.LogInformation("Consuming {contract} for auction {AuctionId}", nameof(AuctionFinished), context.Message.AuctionId);
        
        var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);

        if (context.Message.ItemSold)
        {
            auction.Winner = context.Message.Winner;
            auction.SoldAmount = (int)context.Message.Amount;
            
        }
        auction.Status = "Finished";
        await auction.SaveAsync();
    }
}