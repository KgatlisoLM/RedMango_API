using AutoMapper;
using RedMango_API.Models;
using RedMango_API.Models.Dto;

namespace RedMango_API.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<MenuItem, MenuItemDto>()
               .ForMember(x => x.Image, o => o.MapFrom<ImageUrlResolver>()).ReverseMap();
            CreateMap<ShoppingCart, ShoppingCartDto>().ReverseMap();
            CreateMap<CartItem, CartItemDto>().ReverseMap(); 
            CreateMap<OrderHeader , OrderHeaderDto>().ReverseMap();
            CreateMap<OrderDetails, OrderDetailsDto>().ReverseMap();
        }
    }
}
