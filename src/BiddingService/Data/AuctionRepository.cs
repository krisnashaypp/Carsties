using BiddingService.Models;
using MongoDB.Driver;

namespace BiddingService.Data;

public class AuctionRepository
{
    private readonly IMongoCollection<Auction> _auctionCollection;

    public AuctionRepository(IMongoClient client)
    {
        var database = client.GetDatabase("BidDb");
        _auctionCollection = database.GetCollection<Auction>("Auctions");
    }

    public async Task<Auction> GetAuction(string auctionId)
    {
        return await _auctionCollection
            .Find(x => x.Id == auctionId)
            .FirstOrDefaultAsync();
    }

    public async Task SaveAuction(Auction auction, CancellationToken cancellationToken = default)
    {
        await _auctionCollection.InsertOneAsync(auction, null, cancellationToken);
    }
    
    public async Task InsertOrUpdateAuction(Auction auction, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Auction>.Filter.Eq(a => a.Id, auction.Id);
        var options = new ReplaceOptions { IsUpsert = true };
    
        await _auctionCollection.ReplaceOneAsync(filter, auction, options, cancellationToken);
    }

    public async Task<IEnumerable<Auction>> GetFinishedAuctions(CancellationToken cancellationToken = default)
    {
        return await _auctionCollection
            .Find(a => a.AuctionEnd < DateTime.UtcNow && !a.Finished)
            .ToListAsync(cancellationToken);
    }
}