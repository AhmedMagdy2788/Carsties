using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Auction, AuctionDto>().IncludeMembers(a => a.Item);
        CreateMap<Item, AuctionDto>();
        CreateMap<CreateAuctionDto, Auction>()
            .ForMember(
                dest => dest.Item,
                opt =>
                    opt.MapFrom(src => new Item
                    {
                        Make = src.Make,
                        Model = src.Model,
                        Year = src.Year,
                        Color = src.Color,
                        Mileage = src.Mileage,
                        ImageUrl = src.ImageUrl,
                    })
            );
        CreateMap<CreateAuctionDto, Item>();
        CreateMap<UpdateAuctionDto, Auction>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}
