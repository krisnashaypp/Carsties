using AutoMapper;
using Contracts;
using MassTransit;
using SearchService.Data;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionCreatedConsumer(IMapper mapper, ItemRepository itemRepository) : IConsumer<AuctionCreated>
{
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("--> Consuming auction created: " + context.Message.Id);
        
        var item = mapper.Map<Item>(context.Message);

        if (item.Model == "Foo") throw new ArgumentException("cannot sell cars with name of foo");

        await itemRepository.InsertItemAsync(item);
    }
}