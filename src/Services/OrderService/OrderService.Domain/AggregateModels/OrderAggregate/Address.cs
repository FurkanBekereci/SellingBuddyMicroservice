namespace OrderService.Domain.AggregateModels.OrderAggregate
{
    //public class Address : ValueObject  -- Aşağıdaki record tipi bu extend işlemini yaptığı ve value object gibi davrandığı için buna gerek yok
    public record Address
    {
        public string Street { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string Country { get; private set; }
        public string ZipCode { get; private set; }

        public Address()
        {

        }

        public Address(string street, string city, string state, string country, string zipCode)
        {
            Street = street;
            City = city;
            State = state;
            Country = country;
            ZipCode = zipCode;
        }

        //protected override IEnumerable<object> GetEqualityComponents()
        //{
        //    //Using a yield return statetement to return each element at a time
        //    yield return Street;
        //    yield return City;
        //    yield return State;
        //    yield return Country;
        //    yield return ZipCode;
        //}
    }
}
