using AutoMapper;
using Contracts;
using MassTransit;
using SearchService.Data;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionDeletedConsumer(IMapper mapper, ItemRepository itemRepository) : IConsumer<AuctionDeleted>
{
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine("--> Consuming auction deleted: " + context.Message.Id);
        
        await itemRepository.DeleteItemAsync(context.Message.Id);
    }
}