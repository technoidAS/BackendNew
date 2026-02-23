using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.AddressDto
{
    public class CreateAddressDto
    {
        [Required]
        public string AddressDetail { get; set; } = null!;

        [Required]
        public string City { get; set; } = null!;

        [Required]
        public string State { get; set; } = null!;

        [Required]
        public string Pincode { get; set; } = null!;

        // Optional: Used only if JWT token is not present
        public string? UserId { get; set; }
    }
}

