using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace BiddingService.Models;

public class Auction
{
    [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
    public string Id { get; set; }
    public DateTime AuctionEnd { get; set; }
    public string Seller { get; set; }
    public int ReservePrice { get; set; }
    public bool Finished { get; set; }
}