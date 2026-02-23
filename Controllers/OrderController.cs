using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Config;
using Backend.DTOs.OrderDto;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly BackendDbContext _context;

        public OrderController(BackendDbContext context)
        {
            _context = context;
        }

        // POST: api/order - Place a new order
        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> PlaceOrder(PlaceOrderDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

                // Check if user exists
                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Check if address exists and belongs to user
                var address = _context.Addresses.FirstOrDefault(a => a.AddressId == dto.AddressId && a.UserId == userId);
                if (address == null)
                {
                    return BadRequest(new { message = "Address not found or doesn't belong to user" });
                }

                // Get user's cart
                var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId && c.IsActive);
                if (cart == null)
                {
                    return BadRequest(new { message = "No active cart found" });
                }

                // Get cart items
                var cartItems = _context.CartItems
                    .Where(ci => ci.CartId == cart.CartId)
                    .ToList();

                if (cartItems.Count == 0)
                {
                    return BadRequest(new { message = "Cart is empty" });
                }

                // Calculate total amount
                decimal totalAmount = 0;
                var orderItems = new List<OrderItemModal>();

                foreach (var cartItem in cartItems)
                {
                    var product = _context.Products.FirstOrDefault(p => p.ProductId == cartItem.ProductId);
                    if (product == null)
                    {
                        return BadRequest(new { message = $"Product {cartItem.ProductId} not found" });
                    }

                    if (!product.IsAvailable)
                    {
                        return BadRequest(new { message = $"Product {product.ProductName} is not available" });
                    }

                    if (product.Quantity < cartItem.Quantity)
                    {
                        return BadRequest(new { message = $"Insufficient stock for {product.ProductName}" });
                    }

                    var orderItem = new OrderItemModal
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        PriceAtOrder = product.Price
                    };

                    orderItems.Add(orderItem);
                    totalAmount += product.Price * cartItem.Quantity;

                    // Decrease product quantity
                    product.Quantity -= cartItem.Quantity;
                    _context.Products.Update(product);
                }

                // Create order
                var order = new OrderModal
                {
                    UserId = userId.Value,
                    AddressId = dto.AddressId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Status = "Pending",
                    IsPaid = dto.IsPaid,
                    OrderItems = orderItems
                };

                _context.Orders.Add(order);

                // Clear cart by marking as inactive and removing items
                cart.IsActive = false;
                _context.Carts.Update(cart);

                // Remove cart items
                _context.CartItems.RemoveRange(cartItems);

                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, MapToOrderResponseDto(order));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while placing order", error = ex.Message });
            }
        }

        // GET: api/order - Get all orders for current user
        [HttpGet]
        public ActionResult<List<OrderResponseDto>> GetUserOrders()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

                var orders = _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();

                if (orders.Count == 0)
                {
                    return Ok(new List<OrderResponseDto>());
                }

                var orderResponse = orders.Select(o => MapToOrderResponseDto(o)).ToList();
                return Ok(orderResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/order/{id} - Get order by order ID
        [HttpGet("{id}")]
        public ActionResult<OrderResponseDto> GetOrderById(int id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

                var order = _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefault(o => o.OrderId == id);

                if (order == null)
                {
                    return NotFound(new { message = "Order not found" });
                }

                // Check if order belongs to current user
                if (order.UserId != userId)
                {
                    return Forbid("You don't have permission to access this order");
                }

                return Ok(MapToOrderResponseDto(order));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/order/all (Admin only) - Get all orders in system
        [HttpGet("admin/all")]
        public ActionResult<List<OrderResponseDto>> GetAllOrders()
        {
            try
            {
                var isAdminClaim = User.FindFirst("isAdmin");
                if (isAdminClaim == null || isAdminClaim.Value != "True")
                {
                    return Forbid("Only admins can access all orders");
                }

                var orders = _context.Orders
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();

                var ordersResponse = orders.Select(o => MapToOrderResponseDto(o)).ToList();
                return Ok(ordersResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // PUT: api/order/{id}/status - Update order status (Admin only)
        [HttpPut("{id}/status")]
        public async Task<ActionResult<OrderResponseDto>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            try
            {
                var isAdminClaim = User.FindFirst("isAdmin");
                if (isAdminClaim == null || isAdminClaim.Value != "True")
                {
                    return Forbid("Only admins can update order status");
                }

                var order = _context.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.OrderId == id);
                if (order == null)
                {
                    return NotFound(new { message = "Order not found" });
                }

                order.Status = dto.Status;
                order.IsPaid = dto.IsPaid;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                return Ok(MapToOrderResponseDto(order));
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

        private OrderResponseDto MapToOrderResponseDto(OrderModal order)
        {
            return new OrderResponseDto
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                AddressId = order.AddressId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                IsPaid = order.IsPaid,
                OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    OrderItemId = oi.OrderItemId,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.ProductName ?? "Unknown",
                    Quantity = oi.Quantity,
                    PriceAtOrder = oi.PriceAtOrder
                }).ToList()
            };
        }
    }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = null!;
        public bool IsPaid { get; set; }
    }
}
