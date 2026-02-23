namespace Backend.DTOs.UserDto
{
    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
        public bool isAdmin { get; set; } = false;
    }
}
