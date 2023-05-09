using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Base.Extensions;
using EventBus.Factory;
using OrderService.Api.Extensions;
using OrderService.Api.Extensions.Registrations.ServiceDiscovery;
using OrderService.Api.IntegrationEvents.EventHandlers;
using OrderService.Api.IntegrationEvents.Events;
using OrderService.Application;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Contexts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging(config => config.AddConsole());

builder.Services.RegisterApplicationServices(); //Mediatr burada register ediliyor.
builder.Services.AddPersistenceRegistration(builder.Configuration);
builder.Services.ConfigureEventHandlers(builder.Services.AddTransient);

builder.Services.AddServiceDiscoveryRegistration(builder.Configuration);

//Event bus registration
builder.Services.AddSingleton(sp =>
{
    var config = new EventBusConfig
    {
        ConnectionRetryCount = 5,
        EventNameSuffix = "IntegrationEvent",
        SubscriberClientAppName = "OrderService",
        EventBusType = EventBusType.RabbitMQ
    };

    return EventBusFactory.Create(config, sp);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Services.GetRequiredService<IEventBus>().Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

app.Lifetime.ApplicationStarted.Register(() =>
{
    app.RegisterWithConsul(app.Lifetime);
});


app.MigrateDbContext<OrderDbContext>(async (context) => {

    var services = app.Services;
    var logger = services.GetService<ILogger<OrderDbContext>>();
    var dbContextSeeder = new OrderDbContextSeed();

    await dbContextSeeder.SeedAsync(context, logger);
});

app.Run();
