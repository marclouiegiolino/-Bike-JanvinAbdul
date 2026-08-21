using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Modules.Addresses;
using Api.Modules.Users;
using Api.Modules.Authorizations;
using Api.Main;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // All endpoints require authentication
    public class AddressesController : ControllerBase
    {
        private readonly IAddressRepository _repository;
        private readonly IUserRepository _userRepository;

        public AddressesController(
            IAddressRepository repository,
            IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get all addresses (UserAccess only)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<Address>>> GetAll()
        {
            try
            {
                var addresses = await _repository.GetAllAsync();
                return Ok(addresses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get address by ID (UserAccess only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<Address>> GetById(long id)
        {
            try
            {
                var address = await _repository.GetByIdAsync(id);

                if (address == null)
                    return NotFound(new { message = $"Address with ID {id} not found." });

                return Ok(address);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create new address (UserAccess only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<Address>> Create([FromBody] Address address)
        {
            try
            {
                if (address.UserId <= 0)
                {
                    return BadRequest(new { message = "userId is required and must be greater than 0." });
                }

                var user = await _userRepository.GetByIdAsync((int)address.UserId);
                if (user == null)
                    return BadRequest(new { message = $"User with ID {address.UserId} does not exist." });

                await _repository.AddAsync(address);

                return CreatedAtAction(nameof(GetById), new { id = address.AddressId }, address);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update address (UserAccess only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<IActionResult> Update(long id, [FromBody] Address address)
        {
            try
            {
                if (address.AddressId == 0)
                {
                    address.AddressId = id;
                }

                if (id != address.AddressId)
                    return BadRequest(new { message = "Route id does not match address_id in request body." });

                var existingAddress = await _repository.GetByIdAsync(id);
                if (existingAddress == null)
                    return NotFound(new { message = $"Address with ID {id} not found." });

                if (address.UserId <= 0)
                {
                    return BadRequest(new { message = "userId is required and must be greater than 0." });
                }

                var user = await _userRepository.GetByIdAsync((int)address.UserId);
                if (user == null)
                    return BadRequest(new { message = $"User with ID {address.UserId} does not exist." });

                await _repository.UpdateAsync(address);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete address (UserAccess only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var address = await _repository.GetByIdAsync(id);

                if (address == null)
                    return NotFound(new { message = $"Address with ID {id} not found." });

                await _repository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated addresses (UserAccess only)
        /// </summary>
        [HttpGet("paginated")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<PaginationModel<Address>>> GetPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
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