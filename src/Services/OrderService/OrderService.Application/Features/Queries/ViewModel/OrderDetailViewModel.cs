
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Features.Queries.ViewModel
{
    public class OrderDetailViewModel
    {
        public string OrderNumber { get; init; }
        public DateTime OrderDate { get; init; }
        public string Status { get; init; }
        public string Description { get; init; }
        public string Street { get; init; }
        public string City { get; init; }
        public string ZipCode { get; init; }
        public string Country { get; init; }
        public List<OrderItemViewModel> OrderItems { get; set; }
        public decimal Total { get; set; }
    }

    public class OrderItemViewModel
    {
        public string ProductName { get; init; }
        public int Units { get; init; }
        public double UnitPrice { get; init; }
        public string PictureUrl { get; init; }
    }
}
