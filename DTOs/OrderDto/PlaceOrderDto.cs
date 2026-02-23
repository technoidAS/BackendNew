using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.OrderDto
{
    public class PlaceOrderDto
    {
        [Required]
        public int AddressId { get; set; }

        public bool IsPaid { get; set; } = false;
    }
}
