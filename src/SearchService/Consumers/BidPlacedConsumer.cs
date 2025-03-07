using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private readonly ILogger<BidPlacedConsumer> _logger;

    public BidPlacedConsumer(ILogger<BidPlacedConsumer> logger)
    {
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        _logger.LogInformation("Consuming {contract} for auction {AuctionId}", nameof(BidPlaced), context.Message.AuctionId);

        var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);

        if (ShouldUpdateBid(context, auction))
        {
            auction.CurrentHighBid = context.Message.Amount;
            await auction.SaveAsync();
        }
    }
    
    private bool ShouldUpdateBid(ConsumeContext<BidPlaced> context, Item auction)
    {
        if (auction is null)
        {
            _logger.LogError("Auction with id {MessageAuctionId} not found", context.Message.AuctionId);
            throw new ArgumentException("Auction not found");
        }
        
        var bidWasAccepted = context.Message.BidStatus.Contains("Accepted");
        var newBidHigher = context.Message.Amount > auction.CurrentHighBid;
        return bidWasAccepted && newBidHigher;
    }
}