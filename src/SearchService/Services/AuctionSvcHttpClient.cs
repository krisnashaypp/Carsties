using System.Globalization;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionSvcHttpClient
{
    private readonly HttpClient _httpClient;

    public AuctionSvcHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Item>> GetItemsForSearchDb()
    {
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(item => item.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync();


        string url = $"api/auctions";
        if (!string.IsNullOrEmpty(lastUpdated))
        {
            url += $"?date={lastUpdated}";
        }

        return await _httpClient.GetFromJsonAsync<List<Item>>(url);
    }
}