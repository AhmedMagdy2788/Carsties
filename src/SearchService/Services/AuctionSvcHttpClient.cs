using System;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionSvcHttpClient(HttpClient httpClient)
{
    public async Task<List<Item>> GetItemsFroSearchDb()
    {
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(i => i.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync();

        var url = lastUpdated != null
            ? $"/api/auctions?date={lastUpdated}"
            : "/api/auctions";

        return await httpClient.GetFromJsonAsync<List<Item>>(url)
            ?? new List<Item>();
    }
}
