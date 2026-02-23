namespace Backend.DTOs.ApiResponseDto
{
    public class ApiResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
    }
}
