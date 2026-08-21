using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Modules.CartItems;
using Api.Modules.Users;
using Api.Modules.ProductVariants;
using Api.Modules.Authorizations;
using Api.Main;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // All endpoints require authentication
    public class CartItemsController : ControllerBase
    {
        private readonly ICartItemsRepository _cartItemsRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IUserRepository _userRepository;

        public CartItemsController(
            ICartItemsRepository cartItemsRepository,
            IProductVariantRepository productVariantRepository,
            IUserRepository userRepository)
        {
            _cartItemsRepository = cartItemsRepository;
            _productVariantRepository = productVariantRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get all cart items (UserAccess only)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<CartItems>>> GetAll()
        {
            try
            {
                var cartItems = await _cartItemsRepository.GetAllAsync();
                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get cart item by ID (UserAccess only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<CartItems>> GetById(long id)
        {
            try
            {
                var cartItem = await _cartItemsRepository.GetByIdAsync(id);

                if (cartItem == null)
                    return NotFound(new { message = $"Cart item with ID {id} not found." });

                return Ok(cartItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get cart items by user ID (UserAccess only)
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<CartItems>>> GetByUserId(long userId)
        {
            try
            {
                var cartItems = await _cartItemsRepository.GetByUserIdAsync(userId);
                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get cart items by variant ID (UserAccess only)
        /// </summary>
        [HttpGet("variant/{variantId}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<CartItems>>> GetByVariantId(long variantId)
        {
            try
            {
                var cartItems = await _cartItemsRepository.GetByVariantIdAsync(variantId);
                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create new cart item (UserAccess only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<CartItems>> Create([FromBody] CartItems cartItem)
        {
            try
            {
                if (cartItem.VariantId <= 0)
                {
                    return BadRequest(new { message = "variantId is required and must be greater than 0." });
                }

                var variant = await _productVariantRepository.GetByIdAsync(cartItem.VariantId);
                if (variant == null)
                    return BadRequest(new { message = $"Variant with ID {cartItem.VariantId} does not exist." });

                if (cartItem.UserId <= 0)
                {
                    return BadRequest(new { message = "userId is required and must be greater than 0." });
                }

                var user = await _userRepository.GetByIdAsync((int)cartItem.UserId);
                if (user == null)
                    return BadRequest(new { message = $"User with ID {cartItem.UserId} does not exist." });

                if (cartItem.Quantity < 1 || cartItem.Quantity > 100)
                {
                    return BadRequest(new { message = "Quantity must be between 1 and 100." });
                }

                if (string.IsNullOrWhiteSpace(cartItem.AddedAt))
                {
                    cartItem.AddedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                }

                await _cartItemsRepository.AddAsync(cartItem);

                return CreatedAtAction(nameof(GetById), new { id = cartItem.CartItemId }, cartItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update cart item (UserAccess only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<IActionResult> Update(long id, [FromBody] CartItems cartItem)
        {
            try
            {
                if (cartItem.CartItemId == 0)
                {
                    cartItem.CartItemId = id;
                }

                if (id != cartItem.CartItemId)
                    return BadRequest(new { message = "Route id does not match cart_item_id in request body." });

                var existingCartItem = await _cartItemsRepository.GetByIdAsync(id);
                if (existingCartItem == null)
                    return NotFound(new { message = $"Cart item with ID {id} not found." });

                if (cartItem.VariantId <= 0)
                {
                    return BadRequest(new { message = "variantId is required and must be greater than 0." });
                }

                var variant = await _productVariantRepository.GetByIdAsync(cartItem.VariantId);
                if (variant == null)
                    return BadRequest(new { message = $"Variant with ID {cartItem.VariantId} does not exist." });

                if (cartItem.UserId <= 0)
                {
                    return BadRequest(new { message = "userId is required and must be greater than 0." });
                }

                var user = await _userRepository.GetByIdAsync((int)cartItem.UserId);
                if (user == null)
                    return BadRequest(new { message = $"User with ID {cartItem.UserId} does not exist." });

                if (cartItem.Quantity < 1 || cartItem.Quantity > 100)
                {
                    return BadRequest(new { message = "Quantity must be between 1 and 100." });
                }

                if (string.IsNullOrWhiteSpace(cartItem.AddedAt))
                {
                    cartItem.AddedAt = existingCartItem.AddedAt;
                }

                await _cartItemsRepository.UpdateAsync(cartItem);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete cart item (UserAccess only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var cartItem = await _cartItemsRepository.GetByIdAsync(id);

                if (cartItem == null)
                    return NotFound(new { message = $"Cart item with ID {id} not found." });

                await _cartItemsRepository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated cart items (UserAccess only)
        /// </summary>
        [HttpGet("paginated")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<PaginationModel<CartItems>>> GetPaginated(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            try
            {
                var paged = await _cartItemsRepository.GetPaginatedAsync(pageNumber, pageSize, search);
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
