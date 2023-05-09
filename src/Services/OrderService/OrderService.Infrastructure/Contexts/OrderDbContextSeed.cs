using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Domain.AggregateModels.BuyerAggregate;
using OrderService.Domain.AggregateModels.OrderAggregate;
using OrderService.Domain.SeedWork;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Contexts
{
    public class OrderDbContextSeed
    {
        public async Task SeedAsync(OrderDbContext dbContext, ILogger<OrderDbContext> logger)
        {

            var policy = CreatePolicy(logger, nameof(OrderDbContextSeed));

            await policy.ExecuteAsync(async () =>
            {
                var useCustomizationData = false;

                var contentRootPath = "Seeding/Setup";

                //using (dbContext)
                //{

                //}
                dbContext.Database.Migrate();

                if (!dbContext.CardTypes.Any())
                {
                    dbContext.CardTypes.AddRange(useCustomizationData
                        ? GetCardTypesFromFile(contentRootPath, logger)
                        : GetPredefinedCardTypes()
                        );

                    await dbContext.SaveChangesAsync();
                }

                if (!dbContext.OrderStatuses.Any())
                {
                    dbContext.OrderStatuses.AddRange(useCustomizationData
                        ? GetOrderStatusesFromFile(contentRootPath, logger)
                        : GetPredefinedOrderStatuses()
                        );

                    await dbContext.SaveChangesAsync();
                }
            });
        }

        private IEnumerable<CardType> GetCardTypesFromFile(string contentRootPath, ILogger<OrderDbContext> logger)
        {

            string fileName = "CardTypes.txt";

            if (!File.Exists(contentRootPath))
            {
                return GetPredefinedCardTypes();
            }

            var fileContent = File.ReadAllLines(fileName);

            int id = 1;
            var list = fileContent.Select(i => new CardType(id++, i)).Where(i => i != null);

            return list;

        }

        private IEnumerable<CardType> GetPredefinedCardTypes()
        {
            return Enumeration.GetAll<CardType>();
        }

        private IEnumerable<OrderStatus> GetOrderStatusesFromFile(string contentRootPath, ILogger<OrderDbContext> logger)
        {

            string fileName = "OrderStatus.txt";

            if (!File.Exists(contentRootPath))
            {
                return GetPredefinedOrderStatuses();
            }

            var fileContent = File.ReadAllLines(fileName);

            int id = 1;
            var list = fileContent.Select(i => new OrderStatus(id++, i)).Where(i => i != null);

            return list;

        }

        private IEnumerable<OrderStatus> GetPredefinedOrderStatuses()
        {
            return Enumeration.GetAll<OrderStatus>();
        }

        private AsyncRetryPolicy CreatePolicy(ILogger<OrderDbContext> logger, string prefix, int retries = 3)
        {
            return Policy.Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: retries,
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                    onRetry: (ex, timeSpan, retry, context) =>
                    {
                        logger.LogWarning(ex, "[{prefix}] Exception {ExceptionType} with message {Message}", prefix, ex.GetType().Name);
                    }
                );
        }
    }
}
