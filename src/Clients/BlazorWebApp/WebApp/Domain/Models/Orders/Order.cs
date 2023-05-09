using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace WebApp.Domain.Models.Orders
{
    public class Order
    {
        public string OrderNumber { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public decimal Total { get; set; }
        public string Description { get; set; }
        
        [Required]
        public string City { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        public string State { get; set; }
        
        [Required]
        public string Country { get; set; }
        
        public string ZipCode { get; set; }

        [Required]
        [DisplayName("Card number")]
        public string CardNumber { get; set; }

        [Required]
        [DisplayName("Cardholder name")]
        public string CardHolderName { get; set; }

        public DateTime CardExpiration { get; set; }

        [RegularExpression(@"(0[1-9]|1[0-2])\/[0-9]{2}", ErrorMessage = "Expiration should match a valid MM/YY value")]
        [DisplayName("Card expiration")]
        public string CardExpirationShort { get; set; }

        [Required]
        [DisplayName("Card security number")]
        public string CardSecurityNumber { get; set; }

        public int CardTypeId { get; set; }
        public string Buyer { get; set; }

        public List<OrderItem> OrderItems { get; } = new List<OrderItem>();

        public void CardExpirationShortFormat()
        {
            CardExpirationShort = CardExpiration.ToString("MM/yy");
        }

        public void CardExpirationApiFormat()
        {

            CultureInfo ci = new CultureInfo(CultureInfo.CurrentCulture.LCID);
            ci.Calendar.TwoDigitYearMax = 2099; // the end year, so it goes from 2000 to 2099.
            CardExpiration = DateTime.ParseExact(CardExpirationShort, "MM/yy", ci);

        }
    }
}
