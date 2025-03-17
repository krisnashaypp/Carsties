using BiddingService.Data;
using BiddingService.Models;
using Contracts;
using MassTransit;

namespace BiddingService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly AuctionRepository _auctionRepo;

    public AuctionCreatedConsumer(AuctionRepository auctionRepo)
    {
        _auctionRepo = auctionRepo;
    }

    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        var auction = new Auction
        {
            Id = context.Message.Id.ToString(),
            Seller = context.Message.Seller,
            AuctionEnd = context.Message.AuctionEnd,
            ReservePrice = context.Message.ReservePrice,
        };

        await _auctionRepo.SaveAuction(auction);
    }
}