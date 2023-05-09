using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.SeedWork
{
    public class BaseEntity
    {

        public virtual Guid Id { get; protected set; }
        public DateTime CreatedDate { get; set; }

        int? _requestHashCode;

        private List<INotification> _domainEvents { get; set; }

        public IReadOnlyCollection<INotification> DomainEvents => _domainEvents?.AsReadOnly();

        public void AddDomainEvent(INotification eventItem)
        {
            (_domainEvents ??= new List<INotification>()).Add(eventItem);
        }

        public void RemoveDomainEvent(INotification eventItem)
        {
            _domainEvents?.Remove(eventItem);
        }

        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }

        public bool IsTransient() => Id == default;

       
        public override bool Equals(object obj)
        {

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            if (ReferenceEquals(this, obj)) return true;

            BaseEntity item = (BaseEntity)obj;

            if (item.IsTransient() || IsTransient()) return false;

            return item.Id == Id;
        }

        public override int GetHashCode()
        {
            if (!IsTransient())
            {
                if(!_requestHashCode.HasValue)
                    _requestHashCode = Id.GetHashCode() ^ 31;

                return _requestHashCode.Value;
            }

            return base.GetHashCode();
        }

        public static bool operator ==(BaseEntity left, BaseEntity right)
        {
            //if (!Equals(left, null)) return Equals(right, null);

            return left?.Equals(right) ?? false;
        }

        public static bool operator !=(BaseEntity left, BaseEntity right)
        {
            return !(left == right);
        }
    }
}
