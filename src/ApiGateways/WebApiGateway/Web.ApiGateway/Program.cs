using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Requester.Middleware;
using System.Text;
using Web.ApiGateway.Controllers;
using Web.ApiGateway.Infrastructure;
using Web.ApiGateway.MinimalAPI.Baskets;
using Web.ApiGateway.Services.Implementations;
using Web.ApiGateway.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
const string LOCALHOST_POLICY_KEY = "localHostPolicy";
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddAuthentication(opts =>
//{
//    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(opt =>
//{
//    opt.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = false,
//        ValidateAudience = false,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AuthConfig:Secret"])),
//    };
//});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IBasketService, BasketService>();
builder.Services.AddScoped<HttpClientDelegationHandler>();

builder.Services.AddHttpClient("basket", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Urls:Basket"]);
}).AddHttpMessageHandler<HttpClientDelegationHandler>();

builder.Services.AddHttpClient("catalog", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Urls:Catalog"]);
});

builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    config
        .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
        .AddJsonFile("Configurations/ocelot.json")
        .AddEnvironmentVariables();
});

builder.Services.AddCors(opts =>
{
    opts.AddPolicy(LOCALHOST_POLICY_KEY, policy => policy.WithOrigins("http://localhost:2000").AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});

builder.Services.AddOcelot().AddConsul();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseCors(LOCALHOST_POLICY_KEY);

app.UseBasketMinimalApi();

await app.UseOcelot();

app.Run();


