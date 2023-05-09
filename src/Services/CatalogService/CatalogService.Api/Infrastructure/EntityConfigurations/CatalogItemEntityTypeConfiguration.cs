using CatalogService.Api.Core.Domain;
using CatalogService.Api.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Api.Infrastructure.EntityConfigurations
{
    public class CatalogItemEntityTypeConfiguration : IEntityTypeConfiguration<CatalogItem>
    {
        public void Configure(EntityTypeBuilder<CatalogItem> builder)
        {
            builder.ToTable("Catalog", CatalogContext.DEFAULT_SCHEMA);

            builder.Property(c => c.Id)
                .UseHiLo("catalog_hilo")
                .IsRequired();

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Price)
                .IsRequired();

            builder.Property(c => c.AvailableStock)
                .IsRequired();

            builder.Property(c => c.OnReorder)
                .IsRequired();

            builder.Property(c => c.PictureFileName)
                .IsRequired(false);

            builder.Ignore(c => c.PictureUrl);

            builder.HasOne(c => c.CatalogBrand)
                .WithMany()
                .HasForeignKey(c => c.CatalogBrandId);

            builder.HasOne(c => c.CatalogType)
                .WithMany()
                .HasForeignKey(c => c.CatalogTypeId);
        
        }
    }
}
