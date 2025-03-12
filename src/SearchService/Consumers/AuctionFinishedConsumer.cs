using Contracts;
using MassTransit;
using MongoDB.Driver;
using SearchService.Data;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    private readonly ILogger<AuctionFinishedConsumer> _logger;
    private readonly ItemRepository _itemRepository;

    public AuctionFinishedConsumer(ILogger<AuctionFinishedConsumer> logger, ItemRepository itemRepository)
    {
        _logger = logger;
        _itemRepository = itemRepository;
    }
    
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        _logger.LogInformation("Consuming {contract} for auction {AuctionId}", nameof(AuctionFinished), context.Message.AuctionId);
        
        var auction = await _itemRepository.GetItemAsync(context.Message.AuctionId);
        var filter = Builders<Item>.Filter.Eq(i => i.Id, context.Message.AuctionId);
        
        var update = Builders<Item>.Update
            .Set(i => i.Status, "Finished");
        if (context.Message.ItemSold)
        {
            update.Set(x => x.Winner, auction.Winner);
            update.Set(x => x.SoldAmount, (int)context.Message.Amount);
            
        }
        
        var result = await _itemRepository.UpdateOneAsync(filter, update);
        
        if (result.MatchedCount == 0)
            throw new MessageException(typeof(AuctionUpdated), "Problem updating mongodb - item not found");

        if (result.ModifiedCount == 0)
            Console.WriteLine($"No changes made to auction {context.Message.AuctionId}");
    }
}