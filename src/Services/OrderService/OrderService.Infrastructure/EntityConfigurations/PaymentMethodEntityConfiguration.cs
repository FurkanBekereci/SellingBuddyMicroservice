using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.AggregateModels.BuyerAggregate;
using OrderService.Infrastructure.Contexts;

namespace OrderService.Infrastructure.EntityConfigurations
{
    public class PaymentMethodEntityConfiguration : IEntityTypeConfiguration<PaymentMethod>
    {
        public void Configure(EntityTypeBuilder<PaymentMethod> builder)
        {
            builder.ToTable("paymentmethods", OrderDbContext.DEFAULT_SCHEMA);

            builder.Ignore(b => b.DomainEvents);

            builder.HasKey(ct => ct.Id);
            builder.Property(ct => ct.Id).HasColumnName("id").ValueGeneratedOnAdd();

            builder.Property<int>("BuyerId").IsRequired();

            builder.Property(i => i.CardHolderName)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("CardHolderName")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(i => i.Alias)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("Alias")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(i => i.CardNumber)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("CardNumber")
                .HasMaxLength(25)
                .IsRequired();

            builder.Property(i => i.Expiration)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("Expiration")
                .HasMaxLength(25)
                .IsRequired();

            builder.Property(i => i.CardTypeId)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("CardTypeId")
                .IsRequired();

            builder.HasOne(p => p.CardType)
                .WithMany()
                .HasForeignKey(i => i.CardTypeId);
        }
    }

}
