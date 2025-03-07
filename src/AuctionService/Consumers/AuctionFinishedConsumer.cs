using AuctionService.Data;
using AuctionService.Entities;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    private readonly AuctionDbContext _dbContext;
    private readonly ILogger<AuctionFinishedConsumer> _logger;

    public AuctionFinishedConsumer(AuctionDbContext dbContext, ILogger<AuctionFinishedConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        _logger.LogInformation("Consuming {contract} for auction {AuctionId}", nameof(AuctionFinished), context.Message.AuctionId);
        var auction = await _dbContext.Auctions.FindAsync(context.Message.AuctionId);

        if (auction is null)
        {
            _logger.LogError("Auction with id {MessageAuctionId} not found", context.Message.AuctionId);
            throw new ArgumentException("Auction not found");
        }

        if (context.Message.ItemSold)
        {
            auction.Winner = context.Message.Winner;
            auction.SoldAmount = context.Message.Amount;
        }

        auction.Status = auction.SoldAmount >= auction.ReservePrice
            ? Status.Finished
            : Status.ReserveNotMet;
        
        await _dbContext.SaveChangesAsync();
    }
}