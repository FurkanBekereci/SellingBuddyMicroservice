using BasketService.Api.Core.Application.Repository;
using BasketService.Api.Core.Application.Services;
using BasketService.Api.Extensions;
using BasketService.Api.Infrastructure.Repository;
using BasketService.Api.IntegrationEvents.EventHandlers;
using BasketService.Api.IntegrationEvents.Events;
using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Configuration.AddConfiguration(new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("Configurations/appsettings.json", false)
    .AddJsonFile("Configurations/appsettings.Development.json", true)
    .AddJsonFile("Configurations/appsettings.Production.json", true)
    .AddEnvironmentVariables()
    .Build()
    );

builder.Services.ConfigureAuth(builder.Configuration);
builder.Services.AddSingleton(sp => sp.ConfigureRedis(builder.Configuration));
builder.Services.AddTransient<IBasketRepository, BasketRepository>();
builder.Services.AddTransient<IIdentityService,IdentityService>();
builder.Services.AddTransient<OrderCreatedIntegrationEventHandler>();
builder.Services.ConfigureConsul(builder.Configuration);

//builder.Logging.ClearProviders();
//builder.Logging.AddSerilog(
//    new LoggerConfiguration()
//    .ReadFrom.Configuration(
//        new ConfigurationBuilder()
//        .SetBasePath(Directory.GetCurrentDirectory())
//        .AddJsonFile("Configurations/serliog.json", false)
//        .AddJsonFile("Configurations/serilog.Development.json", true)
//        .AddJsonFile("Configurations/serilog.Production.json", true)
//        .AddEnvironmentVariables()
//        .Build()
//    ).CreateLogger()
//);

builder.Services.AddSingleton(sp =>
{

    var eventBusConfig = new EventBusConfig
    {
        ConnectionRetryCount = 5,
        EventNameSuffix = "IntegrationEvent",
        SubscriberClientAppName = "BasketService",
        EventBusType = EventBusType.RabbitMQ
    };

    return EventBusFactory.Create(eventBusConfig, sp);
});

var app = builder.Build();

//app.UseSerilogRequestLogging(opts =>
//{
//    opts.
//});

app.Services.GetRequiredService<IEventBus>().Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
{
    app.RegisterWithConsul(app.Lifetime);
});

app.Run();
