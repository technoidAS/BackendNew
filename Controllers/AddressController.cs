using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Config;
using Backend.DTOs.AddressDto;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly BackendDbContext _context;

        public AddressController(BackendDbContext context)
        {
            _context = context;
        }

        // POST: api/address - Add address for current user
        [HttpPost]
        public async Task<ActionResult<AddressResponseDto>> AddAddress(CreateAddressDto dto)
        {
            try
            {
                int? userId = null;

                var tokenUserId = GetUserIdFromToken();
                if (tokenUserId != null)
                {
                    userId = tokenUserId;
                }
                else if (!string.IsNullOrEmpty(dto.UserId))
                {
                    if (int.TryParse(dto.UserId, out int parsedUserId))
                    {
                        userId = parsedUserId;
                    }
                    else
                    {
                        return BadRequest(new { message = "Invalid userId format. Must be a valid integer." });
                    }
                }

                if (userId == null)
                {
                    return Unauthorized(new { message = "Authentication required. Please provide a valid JWT token or userId." });
                }

                var existingAddress = _context.Addresses.FirstOrDefault(a => a.UserId == userId);
                if (existingAddress != null)
                {
                    return BadRequest(new { message = "User already has an address. Please update the existing address instead." });
                }

                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                {
                    return NotFound(new { message = $"User with ID {userId} not found." });
                }

                if (string.IsNullOrWhiteSpace(dto.AddressDetail) || 
                    string.IsNullOrWhiteSpace(dto.City) || 
                    string.IsNullOrWhiteSpace(dto.State) || 
                    string.IsNullOrWhiteSpace(dto.Pincode))
                {
                    return BadRequest(new { message = "All address fields (addressDetail, city, state, pincode) are required." });
                }

                var address = new AddressModal
                {
                    UserId = userId.Value,
                    AddressDetail = dto.AddressDetail.Trim(),
                    City = dto.City.Trim(),
                    State = dto.State.Trim(),
                    Pincode = dto.Pincode.Trim()
                };

                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAddress), new { id = address.AddressId }, new AddressResponseDto
                {
                    AddressId = address.AddressId,
                    UserId = address.UserId,
                    AddressDetail = address.AddressDetail,
                    City = address.City,
                    State = address.State,
                    Pincode = address.Pincode
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding address", error = ex.Message });
            }
        }

        // GET api/address - Get current user's address
        [HttpGet]
        public ActionResult<AddressResponseDto> GetUserAddress()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

                var address = _context.Addresses.FirstOrDefault(a => a.UserId == userId);
                if (address == null)
                {
                    return NotFound(new { message = "Address not found" });
                }

                return Ok(new AddressResponseDto
                {
                    AddressId = address.AddressId,
                    UserId = address.UserId,
                    AddressDetail = address.AddressDetail,
                    City = address.City,
                    State = address.State,
                    Pincode = address.Pincode
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/address/{id} - Get address by ID
        [HttpGet("{id}")]
        public ActionResult<AddressResponseDto> GetAddress(int id)
        {
            try
            {
                var address = _context.Addresses.FirstOrDefault(a => a.AddressId == id);
                if (address == null)
                {
                    return NotFound(new { message = "Address not found" });
                }

                return Ok(new AddressResponseDto
                {
                    AddressId = address.AddressId,
                    UserId = address.UserId,
                    AddressDetail = address.AddressDetail,
                    City = address.City,
                    State = address.State,
                    Pincode = address.Pincode
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // PUT: api/address - Update current user's address
        [HttpPut]
        public async Task<ActionResult<AddressResponseDto>> UpdateAddress(UpdateAddressDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

                var address = _context.Addresses.FirstOrDefault(a => a.UserId == userId);
                if (address == null)
                {
                    return NotFound(new { message = "Address not found" });
                }

                // Update address
                address.AddressDetail = dto.AddressDetail;
                address.City = dto.City;
                address.State = dto.State;
                address.Pincode = dto.Pincode;

                _context.Addresses.Update(address);
                await _context.SaveChangesAsync();

                return Ok(new AddressResponseDto
                {
                    AddressId = address.AddressId,
                    UserId = address.UserId,
                    AddressDetail = address.AddressDetail,
                    City = address.City,
                    State = address.State,
                    Pincode = address.Pincode
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // DELETE: api/address - Delete current user's address
        [HttpDelete]
        public async Task<ActionResult> DeleteAddress()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

                var address = _context.Addresses.FirstOrDefault(a => a.UserId == userId);
                if (address == null)
                {
                    return NotFound(new { message = "Address not found" });
                }

                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Address deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
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
