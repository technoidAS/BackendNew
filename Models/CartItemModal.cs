using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class CartItemModal
    {
        [Key]
        public int CartItemId { get; set; }

        public int CartId { get; set; }
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public CartModal Cart { get; set; } = null!;
        public ProductModal Product { get; set; } = null!;
    }
}
