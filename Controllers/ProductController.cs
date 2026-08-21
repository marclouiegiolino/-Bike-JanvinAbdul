using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Modules.Product;
using Api.Modules.Users;
using Api.Modules.Categories;
using Api.Modules.Authorizations;
using Api.Main;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // All endpoints require authentication
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly ICategoriesRepository _categoriesRepository;

        public ProductController(
            IProductRepository repository,
            ICategoriesRepository categoriesRepository)
        {
            _repository = repository;
            _categoriesRepository = categoriesRepository;
        }

        /// <summary>
        /// Get all products — all authenticated users
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            try
            {
                var products = await _repository.GetAllAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get product by ID — all authenticated users
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<Product>> GetById(long id)
        {
            try
            {
                var product = await _repository.GetByIdAsync(id);

                if (product == null)
                    return NotFound(new { message = $"Product with ID {id} not found." });

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create new product — Admin only
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<ActionResult<Product>> Create([FromBody] Product product)
        {
            try
            {
                if (product.CategoryId <= 0)
                {
                    return BadRequest(new { message = "categoryId is required and must be greater than 0." });
                }

                var category = await _categoriesRepository.GetByIdAsync((int)product.CategoryId);
                if (category == null)
                    return BadRequest(new { message = $"Category with ID {product.CategoryId} does not exist." });

                await _repository.AddAsync(product);

                return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update product — Admin only
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Update(long id, [FromBody] Product product)
        {
            try
            {
                if (product.ProductId == 0)
                {
                    product.ProductId = id;
                }

                if (id != product.ProductId)
                    return BadRequest(new { message = "Route id does not match product_id in request body." });

                var existingProduct = await _repository.GetByIdAsync(id);
                if (existingProduct == null)
                    return NotFound(new { message = $"Product with ID {id} not found." });

                if (product.CategoryId <= 0)
                {
                    return BadRequest(new { message = "categoryId is required and must be greater than 0." });
                }

                var category = await _categoriesRepository.GetByIdAsync((int)product.CategoryId);
                if (category == null)
                    return BadRequest(new { message = $"Category with ID {product.CategoryId} does not exist." });

                await _repository.UpdateAsync(product);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete product — Admin only
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var product = await _repository.GetByIdAsync(id);

                if (product == null)
                    return NotFound(new { message = $"Product with ID {id} not found." });

                await _repository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated products — all authenticated users
        /// </summary>
        [HttpGet("paginated")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<PaginationModel<Product>>> GetPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var paged = await _repository.GetPaginatedAsync(pageNumber, pageSize);
                return Ok(paged);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new { status = "API running", timestamp = DateTime.UtcNow });
        }
    }
}