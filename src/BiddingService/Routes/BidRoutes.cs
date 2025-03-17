using System.Security.Claims;
using AutoMapper;
using BiddingService.Data;
using BiddingService.DTOs;
using BiddingService.Models;
using BiddingService.Services;
using Contracts;
using MassTransit;

namespace BiddingService.Routes;

public static class BidRoutes
{
    public static void MapBidRoutes(this WebApplication app)
    {
        var bidGroup = app.MapGroup("/api/bids");

        bidGroup.MapPost("", PlaceBid)
            .RequireAuthorization();
        bidGroup.MapGet("{auctionId}", GetBidsForAuction);

    }
    
    private static async Task<IResult> PlaceBid(
        string auctionId, 
        int amount, 
        AuctionRepository auctionRepo, 
        BidRepository bidRepo, 
        ClaimsPrincipal user,
        IMapper mapper,
        IPublishEndpoint publishEndpoint,
        GrpcAuctionClient grpcClient)
    {
        var auction = await auctionRepo.GetAuction(auctionId);

        if (auction == null)
        {
            auction = grpcClient.GetAuction(auctionId);

            if (auction is null) return Results.BadRequest("Cannot accept bids on this auction at this time");
        }

        if (string.IsNullOrEmpty(user?.Identity?.Name))
        {
            return Results.Unauthorized();
        }

        if (auction.Seller == user.Identity?.Name)
        {
            return Results.BadRequest("You cannot bid on your own auction");
        }

        var bid = new Bid
        {
            Amount = amount,
            AuctionId = auctionId,
            Bidder = user?.Identity?.Name,
        };

        if (auction.AuctionEnd < DateTime.UtcNow)
        {
            bid.BidStatus = BidStatus.Finished;
        }
        else
        {
            var highBid = await bidRepo.GetHighestBidOnAuction(auctionId);

            if ((highBid is not null && amount > highBid.Amount) || highBid is null)
            {
                bid.BidStatus = amount > auction.ReservePrice
                    ? BidStatus.Accepted
                    : BidStatus.AcceptedBelowReserve;
            }

            if (highBid is not null && bid.Amount < highBid.Amount)
            {
                bid.BidStatus = BidStatus.TooLow;
            }
        }
        
        await bidRepo.SaveBid(bid);

        await publishEndpoint.Publish(mapper.Map<BidPlaced>(bid));
        
        return Results.Ok(mapper.Map<BidDto>(bid));
    }

    private static async Task<IResult> GetBidsForAuction(
        string auctionId, 
        BidRepository bidRepo,
        IMapper mapper)
    {
        var bids = await bidRepo.GetBidsForAuction(auctionId);
        var mappedBids = mapper.Map<IEnumerable<BidDto>>(bids);
        return Results.Ok(mappedBids);
    }
}