using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.ProductDto
{
    public class UpdateProductDto
    {
        [Required]
        public string ProductName { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public string Category { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; }
    }
}
