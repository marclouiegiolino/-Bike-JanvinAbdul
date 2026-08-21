using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Modules.ProductVariants;
using Api.Modules.Product;
using Api.Modules.Authorizations;
using Api.Main;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // All endpoints require authentication
    public class ProductVariantController : ControllerBase
    {
        private readonly IProductVariantRepository _repository;
        private readonly IProductRepository _productRepository;

        public ProductVariantController(
            IProductVariantRepository repository,
            IProductRepository productRepository)
        {
            _repository = repository;
            _productRepository = productRepository;
        }

        /// <summary>
        /// Get all product variants — all authenticated users
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<ProductVariant>>> GetAll()
        {
            try
            {
                var variants = await _repository.GetAllAsync();
                return Ok(variants);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get product variant by ID — all authenticated users
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<ProductVariant>> GetById(long id)
        {
            try
            {
                var variant = await _repository.GetByIdAsync(id);

                if (variant == null)
                    return NotFound(new { message = $"Product variant with ID {id} not found." });

                return Ok(variant);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get product variants by product ID — all authenticated users
        /// </summary>
        [HttpGet("product/{productId}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<ProductVariant>>> GetByProductId(long productId)
        {
            try
            {
                var variants = await _repository.GetByProductIdAsync(productId);
                return Ok(variants);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create new product variant — Admin only
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<ActionResult<ProductVariant>> Create([FromBody] ProductVariant variant)
        {
            try
            {
                if (variant.ProductId <= 0)
                {
                    return BadRequest(new { message = "productId is required and must be greater than 0." });
                }

                var product = await _productRepository.GetByIdAsync((int)variant.ProductId);
                if (product == null)
                    return BadRequest(new { message = $"Product with ID {variant.ProductId} does not exist." });

                await _repository.AddAsync(variant);

                return CreatedAtAction(nameof(GetById), new { id = variant.VariantId }, variant);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update product variant — Staff and above (price/stock edits)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = AppPolicies.StaffAndAbove)]
        public async Task<IActionResult> Update(long id, [FromBody] ProductVariant variant)
        {
            try
            {
                if (variant.VariantId == 0)
                {
                    variant.VariantId = id;
                }

                if (id != variant.VariantId)
                    return BadRequest(new { message = "Route id does not match variant_id in request body." });

                var existingVariant = await _repository.GetByIdAsync(id);
                if (existingVariant == null)
                    return NotFound(new { message = $"Product variant with ID {id} not found." });

                if (variant.ProductId <= 0)
                {
                    return BadRequest(new { message = "productId is required and must be greater than 0." });
                }

                var product = await _productRepository.GetByIdAsync((int)variant.ProductId);
                if (product == null)
                    return BadRequest(new { message = $"Product with ID {variant.ProductId} does not exist." });

                await _repository.UpdateAsync(variant);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete product variant — Admin only
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var variant = await _repository.GetByIdAsync(id);

                if (variant == null)
                    return NotFound(new { message = $"Product variant with ID {id} not found." });

                await _repository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated product variants — all authenticated users
        /// </summary>
        [HttpGet("paginated")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<PaginationModel<ProductVariant>>> GetPaginated(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            try
            {
                var paged = await _repository.GetPaginatedAsync(pageNumber, pageSize, search);
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
