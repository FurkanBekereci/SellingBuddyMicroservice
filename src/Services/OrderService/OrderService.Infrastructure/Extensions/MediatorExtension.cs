using MediatR;
using OrderService.Domain.SeedWork;
using OrderService.Infrastructure.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Extensions
{
    public static class MediatorExtension
    {
        public static async Task DispatchDomainEventsAsync(this IMediator mediator, OrderDbContext orderDbContext)
        {
            try
            {
                var domainEntities = orderDbContext.ChangeTracker
                                               .Entries<BaseEntity>()
                                               .Where(e => e.Entity.DomainEvents != null && e.Entity.DomainEvents.Any());

                var domainEvents = domainEntities
                                    .SelectMany(x => x.Entity.DomainEvents)
                                    .ToList();

                foreach (var domainEntity in domainEntities)
                {
                    domainEntity.Entity.ClearDomainEvents();
                }

                foreach (var domainEvent in domainEvents)
                {
                    await mediator.Publish(domainEvent);
                }
            }
            catch (Exception ex)
            {
                int i = 1;
            }
            
                                
        } 
    }
}
