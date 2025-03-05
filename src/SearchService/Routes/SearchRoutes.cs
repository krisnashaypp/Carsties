using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Entities;
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
    private static async Task<IResult> SearchItems([AsParameters] SearchParams searchParams)
    {
        var query = DB.PagedSearch<Item, Item>();

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }

        query = searchParams.OrderBy switch
        {
            "make" => query.Sort(x => x.Ascending(a => a.Make)),
            "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
            _ => query.Sort(x => x.Ascending(a => a.AuctionEnd)),
        };

        query = searchParams.FilterBy switch
        {
            "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
            "endingSoon" => query.Match(x =>
                x.AuctionEnd < DateTime.UtcNow.AddHours(6) && x.AuctionEnd > DateTime.UtcNow),
            _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
        };

        if (!string.IsNullOrEmpty(searchParams.Seller)) query.Match(x => x.Seller == searchParams.Seller);
        if (!string.IsNullOrEmpty(searchParams.Winner)) query.Match(x => x.Winner == searchParams.Winner);

        query.PageNumber(searchParams.PageNumber ?? 1);
        query.PageSize(searchParams.PageSize ?? 4);

        var result = await query.ExecuteAsync();

        return Results.Ok(new
        {
            results = result.Results, pageCount = result.PageCount, totalCount = result.TotalCount
        });
    }
}