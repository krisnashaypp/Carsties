using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson;
using MongoDB.Driver;
using SearchService.Data;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Routes;

public static class SearchRoutes
{
    public static void MapSearchRoutes(this WebApplication app)
    {
        var auctionsGroup = app.MapGroup("/api/search");

        auctionsGroup.MapGet("", SearchItems);
    }
    private static async Task<IResult> SearchItems([AsParameters] SearchParams searchParams, ItemRepository itemRepository)
    {
        var (items, totalCount, pageCount) = await itemRepository.SearchItems(searchParams);
 
        return Results.Ok(new {
            results=items,
            totalCount,
            pageCount 
        });
    }
}