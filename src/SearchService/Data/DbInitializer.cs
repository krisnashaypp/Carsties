using System.Text.Json;
using MongoDB.Driver;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ItemRepository>();
        
        await repo.CreateIndexesAsync();
        
        var count = await repo.CountAsync(FilterDefinition<Item>.Empty);

        // Get items from your AuctionSvcHttpClient
        var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
        var items = await httpClient.GetItemsForSearchDb();

        Console.WriteLine(items.Count + " returned from auction service");

        if (count == 0 && items.Any())
        {
            await repo.InsertItemsAsync(items);
        }
    }
}