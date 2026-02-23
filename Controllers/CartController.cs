using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Config;
using Backend.DTOs.CartDto;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly BackendDbContext _context;

        public CartController(BackendDbContext context)
        {
            _context = context;
        }

        // GET: api/cart - Get current user's cart
        [HttpGet]
        public ActionResult<CartResponseDto> GetCart()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

       
                var cart = _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefault(c => c.UserId == userId && c.IsActive);

                if (cart == null)
                {
                    return NotFound(new { message = "No active cart found. Please add items to create a cart." });
                }

                return Ok(MapToCartResponseDto(cart));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        
        [HttpPost("add")]
        public async Task<ActionResult<CartResponseDto>> AddToCart(AddToCartDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

              
                var product = _context.Products.FirstOrDefault(p => p.ProductId == dto.ProductId);
                if (product == null)
                {
                    return NotFound(new { message = "Product not found" });
                }

               
                if (!product.IsAvailable)
                {
                    return BadRequest(new { message = $"Product {product.ProductName} is not available" });
                }

                if (product.Quantity < dto.Quantity)
                {
                    return BadRequest(new { message = $"Insufficient stock. Only {product.Quantity} available." });
                }

               
                var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId && c.IsActive);
                
                if (cart == null)
                {
                    cart = new CartModal
                    {
                        UserId = userId.Value,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }

               
                var existingCartItem = _context.CartItems
                    .FirstOrDefault(ci => ci.CartId == cart.CartId && ci.ProductId == dto.ProductId);

                if (existingCartItem != null)
                {
                    // Update quantity if product already in cart
                    existingCartItem.Quantity += dto.Quantity;

                    if (product.Quantity < existingCartItem.Quantity)
                    {
                        return BadRequest(new { message = $"Cannot add {dto.Quantity} items. Only {product.Quantity} available in stock." });
                    }

                    _context.CartItems.Update(existingCartItem);
                }
                else
                {
                    // Add new cart item
                    var cartItem = new CartItemModal
                    {
                        CartId = cart.CartId,
                        ProductId = dto.ProductId,
                        Quantity = dto.Quantity
                    };
                    _context.CartItems.Add(cartItem);
                }

                await _context.SaveChangesAsync();

                // Reload cart with items
                cart = _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefault(c => c.CartId == cart.CartId);

                return Ok(MapToCartResponseDto(cart));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // PUT: api/cart/item/{cartItemId} - Update cart item quantity
        [HttpPut("item/{cartItemId}")]
        public async Task<ActionResult<CartResponseDto>> UpdateCartItem(int cartItemId, UpdateCartItemDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

                // Get cart item
                var cartItem = _context.CartItems
                    .Include(ci => ci.Cart)
                    .Include(ci => ci.Product)
                    .FirstOrDefault(ci => ci.CartItemId == cartItemId);

                if (cartItem == null)
                {
                    return NotFound(new { message = "Cart item not found" });
                }

                // Verify cart belongs to user
                if (cartItem.Cart.UserId != userId)
                {
                    return Forbid("You don't have permission to modify this cart");
                }

                // Check product stock
                var product = cartItem.Product;
                if (product.Quantity < dto.Quantity)
                {
                    return BadRequest(new { message = $"Insufficient stock. Only {product.Quantity} available." });
                }

                // Update quantity
                cartItem.Quantity = dto.Quantity;
                _context.CartItems.Update(cartItem);
                await _context.SaveChangesAsync();

                // Return updated cart
                var cart = _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefault(c => c.CartId == cartItem.CartId);

                return Ok(MapToCartResponseDto(cart));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // DELETE: api/cart/item/{cartItemId} - Remove item from cart
        [HttpDelete("item/{cartItemId}")]
        public async Task<ActionResult<CartResponseDto>> RemoveFromCart(int cartItemId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

                // Get cart item
                var cartItem = _context.CartItems
                    .Include(ci => ci.Cart)
                    .FirstOrDefault(ci => ci.CartItemId == cartItemId);

                if (cartItem == null)
                {
                    return NotFound(new { message = "Cart item not found" });
                }

                // Verify cart belongs to user
                if (cartItem.Cart.UserId != userId)
                {
                    return Forbid("You don't have permission to modify this cart");
                }

                var cartId = cartItem.CartId;

                // Remove item
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                // Return updated cart
                var cart = _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefault(c => c.CartId == cartId);

                if (cart != null)
                {
                    return Ok(MapToCartResponseDto(cart));
                }

                return Ok(new { message = "Item removed from cart" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // DELETE: api/cart/clear - Clear entire cart
        [HttpDelete("clear")]
        public async Task<ActionResult> ClearCart()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

                // Get user's active cart
                var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId && c.IsActive);
                if (cart == null)
                {
                    return NotFound(new { message = "No active cart found" });
                }

                // Get all cart items
                var cartItems = _context.CartItems.Where(ci => ci.CartId == cart.CartId).ToList();

                // Remove all items
                _context.CartItems.RemoveRange(cartItems);

                // Mark cart as inactive
                cart.IsActive = false;
                _context.Carts.Update(cart);

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cart cleared successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/cart/count - Get cart item count
        [HttpGet("count")]
        public ActionResult<int> GetCartItemCount()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

                var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId && c.IsActive);
                if (cart == null)
                {
                    return Ok(0);
                }

                var itemCount = _context.CartItems
                    .Where(ci => ci.CartId == cart.CartId)
                    .Sum(ci => ci.Quantity);

                return Ok(itemCount);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/cart/total - Get cart total amount
        [HttpGet("total")]
        public ActionResult<CartTotalDto> GetCartTotal()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

                var cart = _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefault(c => c.UserId == userId && c.IsActive);

                if (cart == null)
                {
                    return NotFound(new { message = "No active cart found" });
                }

                decimal totalAmount = cart.CartItems
                    .Sum(ci => ci.Product.Price * ci.Quantity);

                return Ok(new CartTotalDto
                {
                    CartId = cart.CartId,
                    ItemCount = cart.CartItems.Count,
                    TotalQuantity = cart.CartItems.Sum(ci => ci.Quantity),
                    TotalAmount = totalAmount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // Helper methods
        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("id");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        private CartResponseDto MapToCartResponseDto(CartModal cart)
        {
            decimal totalAmount = 0;
            var cartItems = new List<CartItemResponseDto>();

            foreach (var item in cart.CartItems)
            {
                var subtotal = item.Product.Price * item.Quantity;
                totalAmount += subtotal;

                cartItems.Add(new CartItemResponseDto
                {
                    CartItemId = item.CartItemId,
                    ProductId = item.ProductId,
                    ProductName = item.Product.ProductName,
                    Price = item.Product.Price,
                    Quantity = item.Quantity,
                    Subtotal = subtotal
                });
            }

            return new CartResponseDto
            {
                CartId = cart.CartId,
                UserId = cart.UserId,
                IsActive = cart.IsActive,
                CreatedAt = cart.CreatedAt,
                ItemCount = cart.CartItems.Count,
                TotalAmount = totalAmount,
                CartItems = cartItems
            };
        }
    }

    public class CartTotalDto
    {
        public int CartId { get; set; }
        public int ItemCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
