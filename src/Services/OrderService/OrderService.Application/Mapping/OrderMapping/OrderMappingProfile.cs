using AutoMapper;
using OrderService.Application.Features.Commands.CreateOrder;
using OrderService.Application.Features.Queries.ViewModel;
using OrderService.Domain.AggregateModels.OrderAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Mapping.OrderMapping
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            CreateMap<Order, CreateOrderCommand>().ReverseMap();
            CreateMap<OrderItem, OrderItemDTO>().ReverseMap();

            CreateMap<Order, OrderDetailViewModel>()
                .ForMember(orderViewModel => orderViewModel.City, sourceConfig => sourceConfig.MapFrom(order => order.Address.City))
                .ForMember(orderViewModel => orderViewModel.Country, sourceConfig => sourceConfig.MapFrom(order => order.Address.Country))
                .ForMember(orderViewModel => orderViewModel.Street, sourceConfig => sourceConfig.MapFrom(order => order.Address.Street))
                .ForMember(orderViewModel => orderViewModel.ZipCode, sourceConfig => sourceConfig.MapFrom(order => order.Address.ZipCode))
                .ForMember(orderViewModel => orderViewModel.OrderDate, sourceConfig => sourceConfig.MapFrom(order => order.OrderDate))
                .ForMember(orderViewModel => orderViewModel.OrderNumber, sourceConfig => sourceConfig.MapFrom(order => order.Id.ToString()))
                .ForMember(orderViewModel => orderViewModel.Status, sourceConfig => sourceConfig.MapFrom(order => order.OrderStatus.Name))
                .ForMember(orderViewModel => orderViewModel.Total, sourceConfig => sourceConfig.MapFrom(order => order.OrderItems.Sum(orderItem => orderItem.Units * orderItem.UnitPrice)))
                .ReverseMap();

            CreateMap<OrderItem, OrderItemViewModel>();
        }
    }
}
