namespace Backend.DTOs.AddressDto
{
    public class AddressResponseDto
    {
        public int AddressId { get; set; }
        public int UserId { get; set; }
        public string AddressDetail { get; set; } = null!;
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string Pincode { get; set; } = null!;
    }
}
