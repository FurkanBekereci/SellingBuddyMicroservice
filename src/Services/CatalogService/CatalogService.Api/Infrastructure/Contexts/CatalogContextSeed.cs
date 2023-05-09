using CatalogService.Api.Core.Domain;
using Microsoft.Data.SqlClient;
using Polly;
using System.Globalization;
using System.IO.Compression;

namespace CatalogService.Api.Infrastructure.Contexts
{
    public class CatalogContextSeed
    {

        public async Task SeedAsync(CatalogContext context, IWebHostEnvironment environment, ILogger<CatalogContextSeed> logger)
        {
            var policy = Policy.Handle<SqlException>().WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                    onRetry: (ex, timeSpan, retry, ctx) =>
                    {
                        logger.LogWarning(ex, "[{prefix}] Exception {ExceptionType} with message {message} detected on attempt {retry} of");
                    }
                );

            var setupDirPath = Path.Combine(environment.ContentRootPath, "Infrastructure", "Setup", "SeedFiles");
            var picturePath = "Pics";

            await policy.ExecuteAsync(() => ProcessSeeding(context, setupDirPath, picturePath, logger));
        }

        private async Task ProcessSeeding(CatalogContext context, string setupPath, string picturePath, ILogger logger)
        {
            if (!context.CatalogBrands.Any())
            {
                await context.CatalogBrands.AddRangeAsync(GetCatalogBrandsFromFile(setupPath));
                await context.SaveChangesAsync();
            }

            if (!context.CatalogTypes.Any())
            {
                await context.CatalogTypes.AddRangeAsync(GetCatalogTypesFromFile(setupPath));
                await context.SaveChangesAsync();
            }

            if (!context.CatalogItems.Any())
            {
                await context.CatalogItems.AddRangeAsync(GetCatalogItemsFromFile(setupPath, context));
                await context.SaveChangesAsync();

                //GetCatalogItemPictures(setupPath, picturePath);
            }
        }

        private IEnumerable<CatalogBrand> GetCatalogBrandsFromFile(string contentPath)
        {
            IEnumerable<CatalogBrand> GetPreconfiguredCatalogBrands() => new List<CatalogBrand>
            {
                new CatalogBrand { Brand = "Azure" },
                new CatalogBrand { Brand = ".NET" },
                new CatalogBrand { Brand = "Visual Studio" },
                new CatalogBrand { Brand = "SQL Server" },
                new CatalogBrand { Brand = "Other" },
            };

            string fileName = Path.Combine(contentPath, "BrandsTextFile.txt");

            if (!File.Exists(fileName)) return GetPreconfiguredCatalogBrands();


            var fileContent = File.ReadAllLines(fileName)
                                  .Where(i => i != null)
                                  .Select(i => new CatalogBrand { Brand = i.Trim('"').Trim() });

            return fileContent;
        }

        private IEnumerable<CatalogType> GetCatalogTypesFromFile(string contentPath)
        {
            IEnumerable<CatalogType> GetPreconfiguredCatalogTypes() => new List<CatalogType>
            {
                new CatalogType { Type = "Mug" },
                new CatalogType { Type = "T-Shirt" },
                new CatalogType { Type = "Sheet" },
                new CatalogType { Type = "USB Memory Stick" },
            };

            string fileName = Path.Combine(contentPath, "CatalogTypes.txt");

            if (!File.Exists(fileName)) return GetPreconfiguredCatalogTypes();


            var fileContent = File.ReadAllLines(fileName)
                                  .Where(i => i != null)
                                  .Select(i => new CatalogType { Type = i.Trim('"').Trim() });

            return fileContent;
        }

        private IEnumerable<CatalogItem> GetCatalogItemsFromFile(string contentPath, CatalogContext context)
        {
            IEnumerable<CatalogItem> GetPreconfiguredItems() => new List<CatalogItem>
            {
                new CatalogItem{ CatalogTypeId = 2, CatalogBrandId = 2, AvailableStock = 100, Description = ".NET Bot Block Hoodie", Name=""}
            };

            string fileName = Path.Combine(contentPath, "CatalogItems.txt");

            if (!File.Exists(fileName)) return GetPreconfiguredItems();

            var catalogTypeIdLookup = context.CatalogTypes.ToDictionary(ct => ct.Type, ct => ct.Id);
            var catalogBrandIdLookup = context.CatalogBrands.ToDictionary(cb => cb.Brand, cb => cb.Id);

            var fileContent = File.ReadAllLines(fileName)
                                  .Skip(1)
                                  .Select(line => line.Split(','))
                                  .Select(item => new CatalogItem
                                  {
                                      CatalogTypeId = catalogTypeIdLookup[item[0]],
                                      CatalogBrandId = catalogBrandIdLookup[item[1]],
                                      Description = item[2].Trim('"').Trim(),
                                      Name = item[3].Trim('"').Trim(),
                                      Price = decimal.Parse(item[4].Trim('"').Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                                      PictureFileName = item[5].Trim('"').Trim(),
                                      AvailableStock = string.IsNullOrEmpty(item[6]) ? 0 : int.Parse(item[6]),
                                      OnReorder = Convert.ToBoolean(item[7]),
                                  });

            return fileContent;
        }

        private void GetCatalogItemPictures(string contentPath, string picturePath)
        {
            picturePath ??= "pics";

            DirectoryInfo directoryInfo = new DirectoryInfo(picturePath);
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }
            string zipFileCatalogItemPictures = Path.Combine(contentPath, "CatalogItems.zip");
            ZipFile.ExtractToDirectory(zipFileCatalogItemPictures, picturePath);
        }
    }
}
