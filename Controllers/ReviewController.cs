using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Modules.Reviews;
using Api.Modules.Product;
using Api.Modules.Users;
using Api.Modules.Authorizations;
using Api.Main;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // All endpoints require authentication
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;

        public ReviewController(
            IReviewRepository reviewRepository,
            IProductRepository productRepository,
            IUserRepository userRepository)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get all reviews — Staff and above
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AppPolicies.StaffAndAbove)]
        public async Task<ActionResult<IEnumerable<Review>>> GetAll()
        {
            try
            {
                var reviews = await _reviewRepository.GetAllAsync();
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get review by ID — all authenticated users
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<Review>> GetById(long id)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(id);

                if (review == null)
                    return NotFound(new { message = $"Review with ID {id} not found." });

                return Ok(review);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get reviews by product ID — all authenticated users
        /// </summary>
        [HttpGet("product/{productId}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<Review>>> GetByProductId(long productId)
        {
            try
            {
                var reviews = await _reviewRepository.GetByProductIdAsync(productId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get reviews by user ID — all authenticated users
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<Review>>> GetByUserId(long userId)
        {
            try
            {
                var reviews = await _reviewRepository.GetByUserIdAsync(userId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create new review — all authenticated users (customers review purchased products)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<Review>> Create([FromBody] Review review)
        {
            try
            {
                if (review.ProductId <= 0)
                {
                    return BadRequest(new { message = "productId is required and must be greater than 0." });
                }

                var product = await _productRepository.GetByIdAsync(review.ProductId);
                if (product == null)
                    return BadRequest(new { message = $"Product with ID {review.ProductId} does not exist." });

                if (review.UserId <= 0)
                {
                    return BadRequest(new { message = "userId is required and must be greater than 0." });
                }

                var user = await _userRepository.GetByIdAsync((int)review.UserId);
                if (user == null)
                    return BadRequest(new { message = $"User with ID {review.UserId} does not exist." });

                if (review.Rating < 1 || review.Rating > 5)
                {
                    return BadRequest(new { message = "Rating must be between 1 and 5." });
                }

                if (string.IsNullOrWhiteSpace(review.CreatedAt))
                {
                    review.CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                }

                await _reviewRepository.AddAsync(review);

                return CreatedAtAction(nameof(GetById), new { id = review.ReviewId }, review);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update review — all authenticated users (customers edit own reviews)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<IActionResult> Update(long id, [FromBody] Review review)
        {
            try
            {
                if (review.ReviewId == 0)
                {
                    review.ReviewId = id;
                }

                if (id != review.ReviewId)
                    return BadRequest(new { message = "Route id does not match review_id in request body." });

                var existingReview = await _reviewRepository.GetByIdAsync(id);
                if (existingReview == null)
                    return NotFound(new { message = $"Review with ID {id} not found." });

                if (review.ProductId <= 0)
                {
                    return BadRequest(new { message = "productId is required and must be greater than 0." });
                }

                var product = await _productRepository.GetByIdAsync(review.ProductId);
                if (product == null)
                    return BadRequest(new { message = $"Product with ID {review.ProductId} does not exist." });

                if (review.UserId <= 0)
                {
                    return BadRequest(new { message = "userId is required and must be greater than 0." });
                }

                var user = await _userRepository.GetByIdAsync((int)review.UserId);
                if (user == null)
                    return BadRequest(new { message = $"User with ID {review.UserId} does not exist." });

                if (review.Rating < 1 || review.Rating > 5)
                {
                    return BadRequest(new { message = "Rating must be between 1 and 5." });
                }

                if (string.IsNullOrWhiteSpace(review.CreatedAt))
                {
                    review.CreatedAt = existingReview.CreatedAt;
                }

                await _reviewRepository.UpdateAsync(review);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete review — Staff and above (remove inappropriate reviews)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = AppPolicies.StaffAndAbove)]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(id);

                if (review == null)
                    return NotFound(new { message = $"Review with ID {id} not found." });

                await _reviewRepository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated reviews — Staff and above
        /// </summary>
        [HttpGet("paginated")]
        [Authorize(Policy = AppPolicies.StaffAndAbove)]
        public async Task<ActionResult<PaginationModel<Review>>> GetPaginated(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            try
            {
                var paged = await _reviewRepository.GetPaginatedAsync(pageNumber, pageSize, search);
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
