using Contracts;
using MassTransit;
using MongoDB.Driver;
using SearchService.Data;
using SearchService.Models;

namespace SearchService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private readonly ILogger<BidPlacedConsumer> _logger;
    private readonly ItemRepository _itemRepository;

    public BidPlacedConsumer(ILogger<BidPlacedConsumer> logger, ItemRepository itemRepository)
    {
        _logger = logger;
        _itemRepository = itemRepository;
    }
    
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        _logger.LogInformation("Consuming {contract} for auction {AuctionId}", nameof(BidPlaced), context.Message.AuctionId);
        
        var auction = await _itemRepository.GetItemAsync(context.Message.AuctionId);

        if (ShouldUpdateBid(context, auction))
        {
            auction.CurrentHighBid = context.Message.Amount;
            await _itemRepository.UpdateOneAsync(
                Builders<Item>.Filter.Eq(i => i.Id, auction.Id),
                Builders<Item>.Update.Set(i => i.CurrentHighBid, context.Message.Amount));
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