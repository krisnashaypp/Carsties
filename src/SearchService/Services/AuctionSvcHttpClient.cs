using System.Globalization;
using MongoDB.Driver;
using SearchService.Data;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionSvcHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ItemRepository _itemRepository;

    public AuctionSvcHttpClient(HttpClient httpClient, ItemRepository itemRepository)
    {
        _httpClient = httpClient;
        _itemRepository = itemRepository;
    }

    public async Task<List<Item>> GetItemsForSearchDb()
    {
        string lastUpdated = null;
        
        lastUpdated = await _itemRepository.GetLastUpdatedDate();

        string url = "api/auctions";
        if (!string.IsNullOrEmpty(lastUpdated))
        {
            url += $"?date={lastUpdated}";
        }

        // Get items from API
        return await _httpClient.GetFromJsonAsync<List<Item>>(url);
    }
}