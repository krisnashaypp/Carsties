using BiddingService.Models;
using MongoDB.Driver;

namespace BiddingService.Data;

public class BidRepository
{
    private readonly IMongoCollection<Bid> _bidCollection;

    public BidRepository(IMongoClient client)
    {
        var database = client.GetDatabase("BidDb");
        _bidCollection = database.GetCollection<Bid>("Bids");
    }

    public async Task<Bid> GetHighestBidOnAuction(string auctionId)
    {
        return await _bidCollection.Find(b => b.AuctionId == auctionId)
            .SortByDescending(b => b.Amount)
            .FirstOrDefaultAsync();
    }

    public async Task SaveBid(Bid bid)
    {
        await _bidCollection.InsertOneAsync(bid);
    }

    public async Task<List<Bid>> GetBidsForAuction(string auctionId)
    {
        return await _bidCollection.Find(b => b.AuctionId == auctionId)
            .SortByDescending(b => b.BidTime)
            .ToListAsync();
    }
}