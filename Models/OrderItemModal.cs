using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class OrderItemModal
    {
        [Key]
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }
        public int ProductId { get; set; }

        public int Quantity { get; set; }
        public decimal PriceAtOrder { get; set; }

        public OrderModal Order { get; set; } = null!;
        public ProductModal Product { get; set; } = null!;
    }
}
