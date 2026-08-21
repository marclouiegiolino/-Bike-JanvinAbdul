using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Modules.Shipments;
using Api.Modules.Orders;
using Api.Modules.Authorizations;
using Api.Main;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // All endpoints require authentication
    public class ShipmentController : ControllerBase
    {
        private readonly IShipmentsRepository _repository;
        private readonly IOrderRepository _orderRepository;

        public ShipmentController(
            IShipmentsRepository repository,
            IOrderRepository orderRepository)
        {
            _repository = repository;
            _orderRepository = orderRepository;
        }

        /// <summary>
        /// Get all shipments — Staff and above
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AppPolicies.StaffAndAbove)]
        public async Task<ActionResult<IEnumerable<Shipment>>> GetAll()
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
        /// Get shipment by ID — Staff and above
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = AppPolicies.StaffAndAbove)]
        public async Task<ActionResult<Shipment>> GetById(long id)
        {
            try
            {
                var shipment = await _repository.GetByIdAsync(id);

                if (shipment == null)
                    return NotFound(new { message = $"Shipment with ID {id} not found." });

                return Ok(shipment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get shipments by order ID — all authenticated users (customers track own)
        /// </summary>
        [HttpGet("order/{orderId}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<Shipment>>> GetByOrderId(long orderId)
        {
            try
            {
                var shipments = await _repository.GetByOrderIdAsync(orderId);
                return Ok(shipments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create new shipment — Staff and above
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AppPolicies.StaffAndAbove)]
        public async Task<ActionResult<Shipment>> Create([FromBody] Shipment shipment)
        {
            try
            {
                if (shipment.OrderId <= 0)
                {
                    return BadRequest(new { message = "orderId is required and must be greater than 0." });
                }

                var order = await _orderRepository.GetByIdAsync((int)shipment.OrderId);
                if (order == null)
                    return BadRequest(new { message = $"Order with ID {shipment.OrderId} does not exist." });

                await _repository.AddAsync(shipment);

                return CreatedAtAction(nameof(GetById), new { id = shipment.ShipmentId }, shipment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update shipment tracking/status — Staff and above
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = AppPolicies.StaffAndAbove)]
        public async Task<IActionResult> Update(long id, [FromBody] Shipment shipment)
        {
            try
            {
                if (shipment.ShipmentId == 0)
                {
                    shipment.ShipmentId = id;
                }

                if (id != shipment.ShipmentId)
                    return BadRequest(new { message = "Route id does not match variant_id in request body." });

                var existingShipment = await _repository.GetByIdAsync(id);
                if (existingShipment == null)
                    return NotFound(new { message = $"Shipment with ID {id} not found." });

                if (shipment.OrderId <= 0)
                {
                    return BadRequest(new { message = "orderId is required and must be greater than 0." });
                }

                var order = await _orderRepository.GetByIdAsync((int)shipment.OrderId);
                if (order == null)
                    return BadRequest(new { message = $"Order with ID {shipment.OrderId} does not exist." });

                await _repository.UpdateAsync(shipment);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete shipment — Admin only
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var shipment = await _repository.GetByIdAsync(id);

                if (shipment == null)
                    return NotFound(new { message = $"Shipment with ID {id} not found." });

                await _repository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated shipments — Staff and above
        /// </summary>
        [HttpGet("paginated")]
        [Authorize(Policy = AppPolicies.StaffAndAbove)]
        public async Task<ActionResult<PaginationModel<Shipment>>> GetPaginated(
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

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new { status = "API running", timestamp = DateTime.UtcNow });
        }
    }
}
