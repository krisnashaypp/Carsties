using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Search;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Data;

public class ItemRepository
{
    private readonly IMongoCollection<Item> _collection;

    public ItemRepository(IMongoClient client, IConfiguration configuration)
    {
        var database = client.GetDatabase("SearchDB");
        _collection = database.GetCollection<Item>("Item");
    }

    public async Task CreateIndexesAsync()
    {
        var indexKeys = Builders<Item>.IndexKeys
            .Text(x => x.Make)
            .Text(x => x.Model)
            .Text(x => x.Color);
        var indexModel = new CreateIndexModel<Item>(indexKeys);
        await _collection.Indexes.CreateOneAsync(indexModel);
    }

    public async Task<string> GetLastUpdatedDate()
    {
        var sortDefinition = Builders<Item>.Sort.Descending(item => item.UpdatedAt);
        var projection = Builders<Item>.Projection.Expression(item => item.UpdatedAt.ToString());

        return await _collection
            .Find(Builders<Item>.Filter.Empty)
            .Sort(sortDefinition)
            .Project<string>(projection)
            .FirstOrDefaultAsync();
    }

    public async Task<Item> GetItemAsync(string id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task DeleteItemAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }

    public async Task<UpdateResult> UpdateOneAsync(FilterDefinition<Item> filter, UpdateDefinition<Item> update)
    {
        return await _collection.UpdateOneAsync(filter, update);
    }

    public async Task InsertItemAsync(Item item)
    {
        await _collection.InsertOneAsync(item);
    }

    public async Task InsertItemsAsync(IEnumerable<Item> items)
    {
        await _collection.InsertManyAsync(items);
    }

    public async Task<long> CountAsync(FilterDefinition<Item> filter)
    {
        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task<(List<Item> Items, long TotalCount, int PageCount)> SearchItems(SearchParams searchParams)
    {
        var aggregation = BuildSearchAggregation(searchParams);

        aggregation = ApplySorting(aggregation, searchParams);

        var totalCount = await GetTotalCountAsync(searchParams);

        aggregation = ApplyPagination(aggregation, searchParams);
        var items = await aggregation.ToListAsync();
        
        var pageSize = searchParams.PageSize ?? 4;
        var pageCount = (int)Math.Ceiling((double)totalCount / pageSize);

        return (items, totalCount, pageCount);
    }


    private IAggregateFluent<Item> BuildSearchAggregation(SearchParams searchParams)
    {
        var aggregation = _collection.Aggregate();

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            var searchDefinition = CreateSearchDefinition(searchParams.SearchTerm);
            aggregation = aggregation.Search(searchDefinition, indexName: "car");
        }

        var filter = BuildFilter(searchParams);
        aggregation = aggregation.Match(filter);
        return aggregation;
    }


    private SearchDefinition<Item> CreateSearchDefinition(string searchTerm)
    {
        SearchFuzzyOptions fuzzyOptions = new SearchFuzzyOptions()
        {
            MaxEdits = 1,
            PrefixLength = 1,   
            MaxExpansions = 256
        };
        
        return Builders<Item>.Search
            .Compound()
            .Should(
                Builders<Item>.Search.Autocomplete(x => x.Make, searchTerm, fuzzy: fuzzyOptions),
                Builders<Item>.Search.Autocomplete(x => x.Model, searchTerm, fuzzy: fuzzyOptions),
                Builders<Item>.Search.Autocomplete(x => x.Color, searchTerm, fuzzy: fuzzyOptions)
            );
    }


    private FilterDefinition<Item> BuildFilter(SearchParams searchParams)
    {
        var filterBuilder = Builders<Item>.Filter;
        var filter = filterBuilder.Empty;
        
        filter = searchParams.FilterBy switch
        {
            "finished" => filterBuilder.Lt(x => x.AuctionEnd, DateTime.UtcNow),
            "endingSoon" => filterBuilder.And(
                filterBuilder.Lt(x => x.AuctionEnd, DateTime.UtcNow.AddHours(6)),
                filterBuilder.Gt(x => x.AuctionEnd, DateTime.UtcNow)
            ),
            _ => filterBuilder.Gt(x => x.AuctionEnd, DateTime.UtcNow)
        };
        
        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.Seller, searchParams.Seller));
        }
        
        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.Winner, searchParams.Winner));
        }

        return filter;
    }


    private IAggregateFluent<Item> ApplySorting(IAggregateFluent<Item> aggregation, SearchParams searchParams)
    {
        return searchParams.OrderBy switch
        {
            "make" => aggregation.SortBy(x => x.Make).ThenBy(x => x.Model),
            "new" => aggregation.SortByDescending(x => x.CreatedAt),
            _ => !string.IsNullOrEmpty(searchParams.SearchTerm)
                ? aggregation.AppendStage<Item>(new BsonDocument("$sort",
                    new BsonDocument
                    {
                        { "searchScore", -1 },
                        { "AuctionEnd", 1 }
                    }))
                : aggregation.SortBy(x => x.AuctionEnd)
        };
    }
    
    private async Task<long> GetTotalCountAsync(SearchParams searchParams)
    {
        var countAggregation = _collection.Aggregate();
        
        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            var searchDef = CreateSearchDefinition(searchParams.SearchTerm);
            countAggregation = countAggregation.Search(searchDef, indexName: "car");
        }


        var filter = BuildFilter(searchParams);
        countAggregation = countAggregation.Match(filter);
        
        var result = await countAggregation.Count().FirstOrDefaultAsync();
        return result?.Count ?? 0;
    }


    private IAggregateFluent<Item> ApplyPagination(IAggregateFluent<Item> aggregation, SearchParams searchParams)
    {
        var pageSize = searchParams.PageSize ?? 4;
        var pageNumber = searchParams.PageNumber ?? 1;
        var skip = (pageNumber - 1) * pageSize;

        return aggregation.Skip(skip).Limit(pageSize);
    }
}