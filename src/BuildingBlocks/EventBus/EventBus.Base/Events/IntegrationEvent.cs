using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Events
{
    /// <summary>
    /// Integration Events : 
    /// Sistemde rabbitmq veya azure service bus aracılığıyla
    /// diğer servicelere haber ulaştıran yani servisler arası 
    /// haberleşmeyi sağlayan eventler olarak belirlenmiştir.
    /// </summary>
    public class IntegrationEvent
    {
        [JsonProperty]
        public Guid Id { get; private set; }

        [JsonProperty]
        public DateTime CreatedDate { get; private set; }

        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.Now;
        }

        [JsonConstructor]
        public IntegrationEvent(Guid id, DateTime createdDate)
        {
            Id = id;
            CreatedDate = createdDate;
        }


    }
}
