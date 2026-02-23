using System.Net;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class UserModal
    {
        [Key]
        public int UserId { get; set; }

        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string MobileNo { get; set; } = null!;

        public bool IsAdmin { get; set; } = false;

        public DateTime CreatedAt { get; set; }

        public AddressModal? Address { get; set; }

        
        public CartModal? Cart { get; set; }
        public ICollection<OrderModal> Orders { get; set; } = new List<OrderModal>();
    }
}
