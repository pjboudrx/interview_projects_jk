namespace Domain.DomainClasses
{
    public class OrderItem
    {
        public long item_id { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
    }
}