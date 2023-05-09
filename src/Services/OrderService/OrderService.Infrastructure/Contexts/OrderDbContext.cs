using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.AggregateModels.BuyerAggregate;
using OrderService.Domain.AggregateModels.OrderAggregate;
using OrderService.Domain.SeedWork;
using OrderService.Infrastructure.Extensions;
using System.Reflection;

namespace OrderService.Infrastructure.Contexts
{
    public class OrderDbContext : DbContext , IUnitOfWork
    {

        public const string DEFAULT_SCHEMA = "ordering";
        private readonly IMediator _mediator;

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<PaymentMethod> Payments { get; set; }
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<CardType> CardTypes { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }

        public OrderDbContext() : base()
        {

        }

        public OrderDbContext(DbContextOptions<OrderDbContext> opts, IMediator mediator) : base(opts)
        {
            _mediator = mediator;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _mediator.DispatchDomainEventsAsync(this);

                await base.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
            
        }
    }
}
