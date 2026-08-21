using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Modules.OrderItems;
using Api.Modules.Orders;
using Api.Modules.ProductVariants;
using Api.Modules.Authorizations;
using Api.Main;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // All endpoints require authentication
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductVariantRepository _productVariantRepository;

        public OrderItemController(
            IOrderItemRepository orderItemRepository,
            IOrderRepository orderRepository,
            IProductVariantRepository productVariantRepository)
        {
            _orderItemRepository = orderItemRepository;
            _orderRepository = orderRepository;
            _productVariantRepository = productVariantRepository;
        }

        /// <summary>
        /// Get all order items (UserAccess only)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetAll()
        {
            try
            {
                var items = await _orderItemRepository.GetAllAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get order item by ID (UserAccess only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<OrderItem>> GetById(long id)
        {
            try
            {
                var item = await _orderItemRepository.GetByIdAsync(id);

                if (item == null)
                    return NotFound(new { message = $"Order item with ID {id} not found." });

                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get order items by order ID (UserAccess only)
        /// </summary>
        [HttpGet("order/{orderId}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetByOrderId(long orderId)
        {
            try
            {
                var items = await _orderItemRepository.GetByOrderIdAsync(orderId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get order items by variant ID (UserAccess only)
        /// </summary>
        [HttpGet("variant/{variantId}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetByVariantId(long variantId)
        {
            try
            {
                var items = await _orderItemRepository.GetByVariantIdAsync(variantId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create new order item (UserAccess only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<OrderItem>> Create([FromBody] OrderItem orderItem)
        {
            try
            {
                if (orderItem.OrderId <= 0)
                {
                    return BadRequest(new { message = "orderId is required and must be greater than 0." });
                }

                var order = await _orderRepository.GetByIdAsync(orderItem.OrderId);
                if (order == null)
                    return BadRequest(new { message = $"Order with ID {orderItem.OrderId} does not exist." });

                if (orderItem.VariantId <= 0)
                {
                    return BadRequest(new { message = "variantId is required and must be greater than 0." });
                }

                var productVariant = await _productVariantRepository.GetByIdAsync(orderItem.VariantId);
                if (productVariant == null)
                    return BadRequest(new { message = $"Product variant with ID {orderItem.VariantId} does not exist." });

                if (orderItem.Quantity <= 0)
                {
                    return BadRequest(new { message = "quantity must be greater than 0." });
                }

                await _orderItemRepository.AddAsync(orderItem);

                return CreatedAtAction(nameof(GetById), new { id = orderItem.OrderItemId }, orderItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update order item (UserAccess only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<IActionResult> Update(long id, [FromBody] OrderItem orderItem)
        {
            try
            {
                if (orderItem.OrderItemId == 0)
                {
                    orderItem.OrderItemId = id;
                }

                if (id != orderItem.OrderItemId)
                    return BadRequest(new { message = "Route id does not match order_item_id in request body." });

                var existingItem = await _orderItemRepository.GetByIdAsync(id);
                if (existingItem == null)
                    return NotFound(new { message = $"Order item with ID {id} not found." });

                if (orderItem.OrderId <= 0)
                {
                    return BadRequest(new { message = "orderId is required and must be greater than 0." });
                }

                var order = await _orderRepository.GetByIdAsync(orderItem.OrderId);
                if (order == null)
                    return BadRequest(new { message = $"Order with ID {orderItem.OrderId} does not exist." });

                if (orderItem.VariantId <= 0)
                {
                    return BadRequest(new { message = "variantId is required and must be greater than 0." });
                }

                var productVariant = await _productVariantRepository.GetByIdAsync(orderItem.VariantId);
                if (productVariant == null)
                    return BadRequest(new { message = $"Product variant with ID {orderItem.VariantId} does not exist." });

                if (orderItem.Quantity <= 0)
                {
                    return BadRequest(new { message = "quantity must be greater than 0." });
                }

                await _orderItemRepository.UpdateAsync(orderItem);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete order item (UserAccess only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var item = await _orderItemRepository.GetByIdAsync(id);

                if (item == null)
                    return NotFound(new { message = $"Order item with ID {id} not found." });

                await _orderItemRepository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated order items (UserAccess only)
        /// </summary>
        [HttpGet("paginated")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<PaginationModel<OrderItem>>> GetPaginated(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            try
            {
                var paged = await _orderItemRepository.GetPaginatedAsync(pageNumber, pageSize, search);
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
