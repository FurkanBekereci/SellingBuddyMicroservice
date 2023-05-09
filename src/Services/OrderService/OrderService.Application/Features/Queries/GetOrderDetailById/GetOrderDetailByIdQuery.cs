using AutoMapper;
using MediatR;
using OrderService.Application.Features.Queries.ViewModel;
using OrderService.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Features.Queries.GetOrderDetailById
{
    public class GetOrderDetailByIdQuery : IRequest<OrderDetailViewModel>
    {
        public Guid OrderId { get; set; }

        public GetOrderDetailByIdQuery(Guid orderId)
        {
            OrderId = orderId;
        }

        public class GetOrderDetailByIdQueryHandler : IRequestHandler<GetOrderDetailByIdQuery, OrderDetailViewModel>
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IMapper _mapper;

            public GetOrderDetailByIdQueryHandler(IOrderRepository orderRepository, IMapper mapper)
            {
                _orderRepository = orderRepository;
                _mapper = mapper;
            }

            public async Task<OrderDetailViewModel> Handle(GetOrderDetailByIdQuery request, CancellationToken cancellationToken)
            {
                var order = await _orderRepository.GetByIdAsync(request.OrderId, i => i.OrderItems);
                var result = _mapper.Map<OrderDetailViewModel>(order);

                return result;
            }
        }
    }
}
