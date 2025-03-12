using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Driver;
using SearchService.Data;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer(IMapper mapper, ItemRepository itemRepository) : IConsumer<AuctionUpdated>
{
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine("--> Consuming auction updated: " + context.Message.Id);
        
        var auctionUpdateItem = mapper.Map<Item>(context.Message);
        
        var filter = Builders<Item>.Filter.Eq(i => i.Id, context.Message.Id);

        // Define the update to modify only specific fields
        var update = Builders<Item>.Update
            .Set(i => i.Make, auctionUpdateItem.Make)
            .Set(i => i.Model, auctionUpdateItem.Model)
            .Set(i => i.Year, auctionUpdateItem.Year)
            .Set(i => i.Color, auctionUpdateItem.Color)
            .Set(i => i.Mileage, auctionUpdateItem.Mileage);
        var result = await itemRepository.UpdateOneAsync(filter, update);
        
        if (result.MatchedCount == 0)
            throw new MessageException(typeof(AuctionUpdated), "Problem updating mongodb - item not found");

        if (result.ModifiedCount == 0)
            Console.WriteLine($"No changes made to auction {context.Message.Id}");
    }
}