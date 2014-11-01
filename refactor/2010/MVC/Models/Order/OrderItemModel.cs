using System.ComponentModel.DataAnnotations;

namespace MVC.Models.Order
{
    public class OrderItemModel
    {
        public int quantity { get; set; }

        public decimal price { get; set; }

        public string description { get; set; }

        public long item_id { get; set; }
    }
}