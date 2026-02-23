using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.CartDto
{
    public class UpdateCartItemDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
