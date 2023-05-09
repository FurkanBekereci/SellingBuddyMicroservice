using Ocelot.RequestId;
using Web.ApiGateway.Models.Baskets;
using Web.ApiGateway.Services.Interfaces;

namespace Web.ApiGateway.MinimalAPI.Baskets
{
    public static class BasketApi
    {
        public static IApplicationBuilder UseBasketMinimalApi(this IApplicationBuilder app)
        {

            return app
                .AddBasketItem();
        }

        private static IApplicationBuilder AddBasketItem(this IApplicationBuilder app)
        {
            app.Use((context, next) =>
            {
                if (context.Request.Path.ToString().Contains("/basket/items"))
                {
                    using (var scope = app.ApplicationServices.CreateScope())
                    {
                        var basketService = scope.ServiceProvider.GetRequiredService<IBasketService>();
                        var catalogService = scope.ServiceProvider.GetRequiredService<ICatalogService>();

                        AddBasketItemRequest addBasketItemRequest;

                        try
                        {
                            addBasketItemRequest = context.Request.ReadFromJsonAsync<AddBasketItemRequest>().GetAwaiter().GetResult();
                        }
                        catch (Exception)
                        {
                            addBasketItemRequest = null;
                        }
                        
                        if (addBasketItemRequest is null || addBasketItemRequest.Quantity == 0) return Results.BadRequest("Invalid payload").ExecuteAsync(context);

                        var item = catalogService.GetCatalogItemAsync(addBasketItemRequest.CatalogItemId).GetAwaiter().GetResult();

                        var currentBasket = basketService.GetById(addBasketItemRequest.BasketId).GetAwaiter().GetResult();

                        var product = currentBasket.Items.SingleOrDefault(i => i.ProductId == item.Id);

                        if (product != null)
                        {
                            product.Quantity += addBasketItemRequest.Quantity;
                        }
                        else
                        {
                            currentBasket?.Items.Add(new BasketDataItem
                            {
                                UnitPrice = item.Price,
                                PictureUrl = item.PictureUrl ?? "",
                                ProductId = item.Id,
                                Quantity = addBasketItemRequest.Quantity,
                                Id = Guid.NewGuid().ToString(),
                                ProductName = item.Name ?? ""
                            });
                        }

                        

                        basketService.UpdateAsync(currentBasket).GetAwaiter().GetResult();

                        return Results.Ok().ExecuteAsync(context);

                    }                    
                }

                return next(context);
            });
            return app;
        }
    }
}
