namespace AuctionService.Entities;

public class Item : Entity
{
    public string Make { get; set; } = default!;
    public string Model { get; set; } = default!;
    public int Year { get; set; }
    public string Color { get; set; } = default!;
    public int Mileage { get; set; }
    public string ImageUrl { get; set; } = default!;

    //navigation property
    public Auction Auction { get; set; }
    public Guid AuctionId { get; set; }
}
