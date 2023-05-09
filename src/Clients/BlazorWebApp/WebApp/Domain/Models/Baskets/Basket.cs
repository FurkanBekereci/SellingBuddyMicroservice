namespace WebApp.Domain.Models.Baskets
{
    public class Basket
    {
        public List<BasketItem> Items { get; init; } = new List<BasketItem>();
        public string BuyerId { get; init; }

        public decimal Total => Math.Round(Items.Sum(i => i.UnitPrice * i.Quantity), 2);
    }
}
