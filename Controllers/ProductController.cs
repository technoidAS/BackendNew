using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Config;
using Backend.DTOs.ProductDto;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly BackendDbContext _context;

        public ProductController(BackendDbContext context)
        {
            _context = context;
        }

        // POST: api/product - Add new product (Admin only)
        [HttpPost]
        public async Task<ActionResult<ProductResponseDto>> AddProduct(CreateProductDto dto)
        {
            try
            {
                var isAdminClaim = User.FindFirst("isAdmin");
                if (isAdminClaim == null || isAdminClaim.Value != "True")
                {
                    return Forbid("Only admins can add products");
                }

                var product = new ProductModal
                {
                    ProductName = dto.ProductName,
                    Description = dto.Description,
                    Category = dto.Category,
                    Price = dto.Price,
                    Quantity = dto.Quantity,
                    ImageUrl = dto.ImageUrl,
                    IsAvailable = dto.IsAvailable,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, MapToProductResponseDto(product));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/product - Get all products
        [HttpGet]
        public ActionResult<IEnumerable<ProductResponseDto>> GetAllProducts()
        {
            try
            {
                var products = _context.Products
                    .Select(p => new ProductResponseDto
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        Category = p.Category,
                        Price = p.Price,
                        Quantity = p.Quantity,
                        ImageUrl = p.ImageUrl,
                        IsAvailable = p.IsAvailable,
                        CreatedAt = p.CreatedAt
                    })
                    .ToList();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/product/{id} - Get product by ID
        [HttpGet("{id}")]
        public ActionResult<ProductResponseDto> GetProduct(int id)
        {
            try
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
                if (product == null)
                {
                    return NotFound(new { message = "Product not found" });
                }

                return Ok(MapToProductResponseDto(product));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/product/category/{category} - Get products by category
        [HttpGet("category/{category}")]
        public ActionResult<IEnumerable<ProductResponseDto>> GetProductsByCategory(string category)
        {
            try
            {
                var products = _context.Products
                    .Where(p => p.Category.ToLower() == category.ToLower())
                    .Select(p => new ProductResponseDto
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        Category = p.Category,
                        Price = p.Price,
                        Quantity = p.Quantity,
                        ImageUrl = p.ImageUrl,
                        IsAvailable = p.IsAvailable,
                        CreatedAt = p.CreatedAt
                    })
                    .ToList();

                if (products.Count == 0)
                {
                    return Ok(new List<ProductResponseDto>());
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/product/search - Search products by name or description
        [HttpGet("search")]
        public ActionResult<IEnumerable<ProductResponseDto>> SearchProducts([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { message = "Search query cannot be empty" });
                }

                var products = _context.Products
                    .Where(p => p.ProductName.ToLower().Contains(query.ToLower()) ||
                               (p.Description != null && p.Description.ToLower().Contains(query.ToLower())))
                    .Select(p => new ProductResponseDto
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        Category = p.Category,
                        Price = p.Price,
                        Quantity = p.Quantity,
                        ImageUrl = p.ImageUrl,
                        IsAvailable = p.IsAvailable,
                        CreatedAt = p.CreatedAt
                    })
                    .ToList();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // PUT: api/product/{id} - Update product (Admin only)
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductResponseDto>> UpdateProduct(int id, UpdateProductDto dto)
        {
            try
            {
                var isAdminClaim = User.FindFirst("isAdmin");
                if (isAdminClaim == null || isAdminClaim.Value != "True")
                {
                    return Forbid("Only admins can update products");
                }

                var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
                if (product == null)
                {
                    return NotFound(new { message = "Product not found" });
                }

                product.ProductName = dto.ProductName;
                product.Description = dto.Description;
                product.Category = dto.Category;
                product.Price = dto.Price;
                product.Quantity = dto.Quantity;
                product.ImageUrl = dto.ImageUrl;
                product.IsAvailable = dto.IsAvailable;

                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                return Ok(MapToProductResponseDto(product));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // DELETE: api/product/{id} - Delete product (Admin only)
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            try
            {
                var isAdminClaim = User.FindFirst("isAdmin");
                if (isAdminClaim == null || isAdminClaim.Value != "True")
                {
                    return Forbid("Only admins can delete products");
                }

                var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
                if (product == null)
                {
                    return NotFound(new { message = "Product not found" });
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // Helper method
        private ProductResponseDto MapToProductResponseDto(ProductModal product)
        {
            return new ProductResponseDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                Category = product.Category,
                Price = product.Price,
                Quantity = product.Quantity,
                ImageUrl = product.ImageUrl,
                IsAvailable = product.IsAvailable,
                CreatedAt = product.CreatedAt
            };
        }
    }
}
