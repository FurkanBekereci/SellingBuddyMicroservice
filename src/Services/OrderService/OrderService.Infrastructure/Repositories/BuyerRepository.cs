﻿using OrderService.Application.Interfaces.Repositories;
using OrderService.Domain.AggregateModels.BuyerAggregate;
using OrderService.Infrastructure.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Repositories
{
    public class BuyerRepository : GenericRepository<Buyer, OrderDbContext>, IBuyerRepository
    {
        public BuyerRepository(OrderDbContext dbContext) : base(dbContext)
        {

        }

    }
}
