using BiddingService.Data;
using Contracts;
using MassTransit;

namespace BiddingService.Services;

public class CheckAuctionFinished : BackgroundService
{
    private readonly ILogger<CheckAuctionFinished> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly AuctionRepository _auctionRepo;
    private readonly BidRepository _bidRepository;

    public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider serviceProvider, AuctionRepository auctionRepo, BidRepository bidRepository)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _auctionRepo = auctionRepo;
        _bidRepository = bidRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting check for finished auctions");
        stoppingToken.Register(() => _logger.LogInformation("==> Auction check is stopping"));

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAuctions(stoppingToken);

            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task CheckAuctions(CancellationToken stoppingToken)
    {
        var finishedAuctions = (await _auctionRepo.GetFinishedAuctions(stoppingToken)).ToList();

        if (finishedAuctions.Count == 0) return;
        
        _logger.LogInformation("==> Found {FinishedAuctionsCount} auctions that have completed", finishedAuctions.Count);
        
        using var scope = _serviceProvider.CreateScope();
        var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        
        foreach (var finishedAuction in finishedAuctions)
        {
            finishedAuction.Finished = true;
            await _auctionRepo.InsertOrUpdateAuction(finishedAuction, stoppingToken);

            var winningBid = await _bidRepository.GetHighestBidOnAuction(finishedAuction.Id);

            await endpoint.Publish(new AuctionFinished
            {
                ItemSold = winningBid is not null,
                AuctionId = finishedAuction.Id,
                Winner = winningBid?.Bidder,
                Amount = winningBid?.Amount,
                Seller = finishedAuction.Seller,
            }, stoppingToken);
        }
    }
}