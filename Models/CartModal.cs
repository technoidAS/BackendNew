using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class CartModal
    {
        [Key]
        public int CartId { get; set; }

        public int UserId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public UserModal User { get; set; } = null!;
        public ICollection<CartItemModal> CartItems { get; set; } = new List<CartItemModal>();
    }
}
