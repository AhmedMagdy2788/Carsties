using AuctionService.Enums;

namespace AuctionService.Entities;

public class Auction : Entity
{
    public int ReservePrice { get; set; }
    public string Seller { get; set; } = default!;
    public string? Winner { get; set; }
    public int? SoldAmount { get; set; }
    public int? CurrentHighBid { get; set; }
    public DateTime AuctionEnd { get; set; }
    public Status Status { get; set; }
    public Item Item { get; set; }
}
