namespace Backend.DTOs.OrderDto
{
    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int AddressId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public bool IsPaid { get; set; }
        public ICollection<OrderItemResponseDto> OrderItems { get; set; } = new List<OrderItemResponseDto>();
    }
}
