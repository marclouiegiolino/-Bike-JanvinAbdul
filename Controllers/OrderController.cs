using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Modules.Orders;
using Api.Modules.Users;
using Api.Modules.Authorizations;
using Api.Main;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // All endpoints require authentication
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _repository;
        private readonly IUserRepository _userRepository;

        public OrderController(
            IOrderRepository repository,
            IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get all orders — Staff and above
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AppPolicies.StaffAndAbove)]
        public async Task<ActionResult<IEnumerable<Order>>> GetAll()
        {
            try
            {
                var orders = await _repository.GetAllAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get order by ID — Staff and above
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = AppPolicies.StaffAndAbove)]
        public async Task<ActionResult<Order>> GetById(long id)
        {
            try
            {
                var order = await _repository.GetByIdAsync(id);

                if (order == null)
                    return NotFound(new { message = $"Order with ID {id} not found." });

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get orders by user ID — all authenticated users (customers view own)
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByUser(long userId)
        {
            try
            {
                var orders = await _repository.GetAllAsync();
                var userOrders = orders.Where(o => o.UserId == userId);
                return Ok(userOrders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create new order — all authenticated users (customers place own orders)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<Order>> Create([FromBody] Order order)
        {
            try
            {
                if (order.UserId <= 0)
                {
                    return BadRequest(new { message = "userId is required and must be greater than 0." });
                }

                var user = await _userRepository.GetByIdAsync((int)order.UserId);
                if (user == null)
                    return BadRequest(new { message = $"User with ID {order.UserId} does not exist." });

                await _repository.AddAsync(order);

                return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update order status — Staff and above
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = AppPolicies.StaffAndAbove)]
        public async Task<IActionResult> Update(long id, [FromBody] Order order)
        {
            try
            {
                if (order.OrderId == 0)
                {
                    order.OrderId = id;
                }

                if (id != order.OrderId)
                    return BadRequest(new { message = "Route id does not match order_id in request body." });

                var existingOrder = await _repository.GetByIdAsync(id);
                if (existingOrder == null)
                    return NotFound(new { message = $"Order with ID {id} not found." });

                if (order.UserId <= 0)
                {
                    return BadRequest(new { message = "userId is required and must be greater than 0." });
                }

                var user = await _userRepository.GetByIdAsync((int)order.UserId);
                if (user == null)
                    return BadRequest(new { message = $"User with ID {order.UserId} does not exist." });

                await _repository.UpdateAsync(order);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cancel/delete order — Admin only
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var order = await _repository.GetByIdAsync(id);

                if (order == null)
                    return NotFound(new { message = $"Order with ID {id} not found." });

                await _repository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated orders — Staff and above
        /// </summary>
        [HttpGet("paginated")]
        [Authorize(Policy = AppPolicies.StaffAndAbove)]
        public async Task<ActionResult<PaginationModel<Order>>> GetPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
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