using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Config;
using Backend.DTOs.UserDto;
using Backend.Services;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly BackendDbContext _context;
        private readonly TokenService _tokenService;

        public UserController(BackendDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // POST: api/user/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto dto)
        {
            try
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Email already in use" });
                }

                var passwordHash = HashPassword(dto.Password);

                var user = new UserModal
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                    PasswordHash = passwordHash,
                    MobileNo = dto.PhoneNumber ?? "",
                    IsAdmin = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var token = _tokenService.GenerateToken(user.UserId, user.Email, user.UserName, user.IsAdmin);

                return Ok(new AuthResponseDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during registration", error = ex.Message });
            }
        }

        // POST: api/user/login
        [HttpPost("login")]
        public ActionResult<AuthResponseDto> Login(LoginRequestDto dto)
        {
            try
            {

                var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }


                if (!VerifyPassword(dto.Password, user.PasswordHash))
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }


                var token = _tokenService.GenerateToken(user.UserId, user.Email, user.UserName, user.IsAdmin);

                return Ok(new AuthResponseDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = token,
                    isAdmin = user.IsAdmin
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
            }
        }

        // GET: api/user/profile
        [HttpGet("profile")]
        public ActionResult<UserProfileDto> GetProfile()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new UserProfileDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    MobileNo = user.MobileNo,
                    IsAdmin = user.IsAdmin,
                    CreatedAt = user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public ActionResult<UserProfileDto> GetUser(int id)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new UserProfileDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    MobileNo = user.MobileNo,
                    IsAdmin = user.IsAdmin,
                    CreatedAt = user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/user
        [HttpGet]
        public ActionResult<List<UserProfileDto>> GetAllUsers()
        {
            try
            {
                var users = _context.Users.Select(u => new UserProfileDto
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    Email = u.Email,
                    MobileNo = u.MobileNo,
                    IsAdmin = u.IsAdmin,
                    CreatedAt = u.CreatedAt
                }).ToList();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // PUT: api/user/profile
        [HttpPut("profile")]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile(UpdateUserDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                user.UserName = dto.UserName;
                user.Email = dto.Email;
                user.MobileNo = dto.MobileNo;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new UserProfileDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    MobileNo = user.MobileNo,
                    IsAdmin = user.IsAdmin,
                    CreatedAt = user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // DELETE: api/user/profile
        [HttpDelete("profile")]
        public async Task<ActionResult> DeleteProfile()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // Helper methods
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }

        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("id");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
