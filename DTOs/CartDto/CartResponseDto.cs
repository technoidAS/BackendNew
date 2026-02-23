namespace Backend.DTOs.CartDto
{
    public class CartResponseDto
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public ICollection<CartItemResponseDto> CartItems { get; set; } = new List<CartItemResponseDto>();
    }
}
