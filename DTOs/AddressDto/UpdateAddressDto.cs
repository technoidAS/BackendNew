using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.AddressDto
{
    public class UpdateAddressDto
    {
        [Required]
        public string AddressDetail { get; set; } = null!;

        [Required]
        public string City { get; set; } = null!;

        [Required]
        public string State { get; set; } = null!;

        [Required]
        public string Pincode { get; set; } = null!;
    }
}
