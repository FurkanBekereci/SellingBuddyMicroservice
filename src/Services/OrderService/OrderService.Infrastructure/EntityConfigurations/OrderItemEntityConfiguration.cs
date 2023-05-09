using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.AggregateModels.OrderAggregate;
using OrderService.Infrastructure.Contexts;

namespace OrderService.Infrastructure.EntityConfigurations
{
    public class OrderItemEntityConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("orderItems", OrderDbContext.DEFAULT_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Ignore(i => i.DomainEvents);

            builder.Property(p => p.Id).ValueGeneratedOnAdd();

            builder.Property<int>("OrderId").IsRequired();
        }
    }

}
