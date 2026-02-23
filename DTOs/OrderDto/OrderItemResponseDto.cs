namespace Backend.DTOs.OrderDto
{
    public class OrderItemResponseDto
    {
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal PriceAtOrder { get; set; }
    }
}
