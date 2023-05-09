using Microsoft.EntityFrameworkCore;
using Polly;
using System.Data.SqlClient;

namespace OrderService.Api.Extensions
{
    public static class HostExtension
    {
        public static IHost MigrateDbContext<TContext>(this IHost app, Action<TContext> seedAction) where TContext : DbContext
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();
                var dbContext = services.GetRequiredService<TContext>();

                try
                {
                    logger.LogInformation("Migrating the database associated with context {DbContextName}.", typeof(TContext).Name);

                    var retry = Policy.Handle<SqlException>()
                        .WaitAndRetry(new TimeSpan[]
                        {
                            TimeSpan.FromSeconds(3),
                            TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(8),
                        });

                    retry.Execute(() => InvokeSeeder(seedAction, dbContext));

                    logger.LogInformation("Migration done!!");
                }
                catch (Exception)
                {

                    logger.LogError("An error occured while migration the database used on context {DbContextName}.", typeof(TContext).Name);
                }
            }

            return app;
        }

        private static void InvokeSeeder<TContext>(Action<TContext> seeder, TContext dbContext) where TContext : DbContext
        {
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();
            seeder(dbContext);
        }
    }
}
