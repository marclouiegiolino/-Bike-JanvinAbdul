using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Modules.Wishlist;
using Api.Modules.ProductVariants;
using Api.Modules.Users;
using Api.Modules.Authorizations;
using Api.Main;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // All endpoints require authentication
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IUserRepository _userRepository;

        public WishlistController(
            IWishlistRepository wishlistRepository,
            IProductVariantRepository productVariantRepository,
            IUserRepository userRepository)
        {
            _wishlistRepository = wishlistRepository;
            _productVariantRepository = productVariantRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get all reviews (UserAccess only)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<Wishlist>>> GetAll()
        {
            try
            {
                var reviews = await _wishlistRepository.GetAllAsync();
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get review by ID (UserAccess only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<Wishlist>> GetById(long id)
        {
            try
            {
                var review = await _wishlistRepository.GetByIdAsync(id);

                if (review == null)
                    return NotFound(new { message = $"Review with ID {id} not found." });

                return Ok(review);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpGet("variant/{variantId}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<Wishlist>>> GetByVariantId(long variantId)
        {
            try
            {
                var wishlist = await _wishlistRepository.GetByVariantIdAsync(variantId);
                return Ok(wishlist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<Wishlist>>> GetByUserId(long userId)
        {
            try
            {
                var wishlist = await _wishlistRepository.GetByUserIdAsync(userId);
                return Ok(wishlist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create new review (UserAccess only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<Wishlist>> Create([FromBody] Wishlist wishlist)
        {
            try
            {
                if (wishlist.VariantId <= 0)
                {
                    return BadRequest(new { message = "variantId is required and must be greater than 0." });
                }

                var productVariant = await _productVariantRepository.GetByIdAsync(wishlist.VariantId);
                if (productVariant == null)
                    return BadRequest(new { message = $"Product variant with ID {wishlist.VariantId} does not exist." });

                if (wishlist.UserId <= 0)
                {
                    return BadRequest(new { message = "userId is required and must be greater than 0." });
                }

                var user = await _userRepository.GetByIdAsync((int)wishlist.UserId);
                if (user == null)
                    return BadRequest(new { message = $"User with ID {wishlist.UserId} does not exist." });

                if (string.IsNullOrWhiteSpace(wishlist.AddedAt))
                {
                    wishlist.AddedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                }

                await _wishlistRepository.AddAsync(wishlist);

                return CreatedAtAction(nameof(GetById), new { id = wishlist.WishlistId }, wishlist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<IActionResult> Update(long id, [FromBody] Wishlist wishlist)
        {
            try
            {
                if (wishlist.WishlistId == 0)
                {
                    wishlist.WishlistId = id;
                }

                if (id != wishlist.WishlistId)
                    return BadRequest(new { message = "Route id does not match review_id in request body." });

                var existingWishlist = await _wishlistRepository.GetByIdAsync(id);
                if (existingWishlist == null)
                    return NotFound(new { message = $"Wishlist with ID {id} not found." });

                if (wishlist.VariantId <= 0)
                {
                    return BadRequest(new { message = "variantId is required and must be greater than 0." });
                }

                var productVariant = await _productVariantRepository.GetByIdAsync(wishlist.VariantId);
                if (productVariant == null)
                    return BadRequest(new { message = $"Product variant with ID {wishlist.VariantId} does not exist." });

                if (wishlist.UserId <= 0)
                {
                    return BadRequest(new { message = "userId is required and must be greater than 0." });
                }

                var user = await _userRepository.GetByIdAsync((int)wishlist.UserId);
                if (user == null)
                    return BadRequest(new { message = $"User with ID {wishlist.UserId} does not exist." });

                if (string.IsNullOrWhiteSpace(wishlist.AddedAt))
                {
                    wishlist.AddedAt = existingWishlist.AddedAt;
                }

                await _wishlistRepository.UpdateAsync(wishlist);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var wishlist = await _wishlistRepository.GetByIdAsync(id);

                if (wishlist == null)
                    return NotFound(new { message = $"Wishlist with ID {id} not found." });

                await _wishlistRepository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated wishlist items (UserAccess only)
        /// </summary>
        [HttpGet("paginated")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<PaginationModel<Wishlist>>> GetPaginated(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            try
            {
                var paged = await _wishlistRepository.GetPaginatedAsync(pageNumber, pageSize, search);
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
