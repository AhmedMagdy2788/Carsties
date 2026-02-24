using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SearchController : ControllerBase
{
    // This is a placeholder controller. You can implement search functionality here.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Item>>> SearchItems(
        [FromQuery] SearchParams searchParams
    )
    {
        var query = DB.PagedSearch<Item, Item>();

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }

        query = searchParams.OrderBy switch
        {
            "make" => query.Sort(x =>
                searchParams.SortDescending ? x.Descending(i => i.Make) : x.Ascending(i => i.Make)
            ),
            "model" => query.Sort(x =>
                searchParams.SortDescending ? x.Descending(i => i.Model) : x.Ascending(i => i.Model)
            ),
            "color" => query.Sort(x =>
                searchParams.SortDescending ? x.Descending(i => i.Color) : x.Ascending(i => i.Color)
            ),
            "new" => query.Sort(x =>
                searchParams.SortDescending
                    ? x.Descending(i => i.CreatedAt)
                    : x.Ascending(i => i.CreatedAt)
            ),
            _ => query.Sort(x =>
                searchParams.SortDescending
                    ? x.Descending(i => i.AuctionEnd)
                    : x.Ascending(i => i.AuctionEnd)
            ),
        };

        if (!string.IsNullOrEmpty(searchParams.FilterBy))
        {
            query = searchParams.FilterBy switch
            {
                "endingSoon" => query.Match(i =>
                    i.AuctionEnd <= DateTime.UtcNow.AddHours(6) && i.AuctionEnd > DateTime.UtcNow
                ),
                "finished" => query.Match(i => i.AuctionEnd <= DateTime.UtcNow),
                _ => query.Match(i => i.AuctionEnd > DateTime.UtcNow),
            };
        }

        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(i => i.Seller == searchParams.Seller);
        }

        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(i => i.Winner == searchParams.Winner);
        }

        query.PageNumber(searchParams.PageNumber).PageSize(searchParams.PageSize);
        var results = await query.ExecuteAsync();
        return Ok(
            new
            {
                results = results.Results,
                pageCount = results.PageCount,
                totalCount = results.TotalCount,
            }
        );
    }
}
