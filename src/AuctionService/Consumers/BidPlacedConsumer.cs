using AuctionService.Data;
using AuctionService.Entities;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private readonly AuctionDbContext _dbContext;
    private readonly ILogger<AuctionFinishedConsumer> _logger;

    public BidPlacedConsumer(AuctionDbContext dbContext, ILogger<AuctionFinishedConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        _logger.LogInformation("Consuming {contract} for auction {AuctionId}", nameof(BidPlaced), context.Message.AuctionId);
        var auction = await _dbContext.Auctions.FindAsync(Guid.Parse(context.Message.AuctionId));

        if (ShouldUpdateBid(context, auction))
        {
            auction.CurrentHighBid = context.Message.Amount;
            await _dbContext.SaveChangesAsync();
        }
    }

    private bool ShouldUpdateBid(ConsumeContext<BidPlaced> context, Auction auction)
    {
        if (auction is null)
        {
            _logger.LogError("Auction with id {MessageAuctionId} not found", context.Message.AuctionId);
            throw new ArgumentException("Auction not found");
        }
        
        var auctionNeverBidOn = auction.CurrentHighBid is null;
        var bidWasAccepted = context.Message.BidStatus.Contains("Accepted");
        var newBidHigher = context.Message.Amount > auction.CurrentHighBid;
        return auctionNeverBidOn || (bidWasAccepted && newBidHigher);
    }
}