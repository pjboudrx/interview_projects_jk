using System.Collections.Generic;

namespace Domain.DomainClasses
{
    public class Order
    {
        public long id { get; set; }
        public IList<OrderItem> items { get; set; }

        public Order()
        {
            items = new List<OrderItem>();
        }
    }
}