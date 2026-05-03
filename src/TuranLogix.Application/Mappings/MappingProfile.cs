using AutoMapper;
using TuranLogix.Application.DTOs.Orders;
using TuranLogix.Domain.Entities;

namespace TuranLogix.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Order, OrderSummaryDto>();
        CreateMap<Order, OrderDetailDto>()
            .ForMember(d => d.ClientName, opt => opt.MapFrom(s => s.Client != null ? s.Client.FullName : string.Empty));
    }
}
