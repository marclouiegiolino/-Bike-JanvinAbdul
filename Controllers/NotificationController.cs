using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Modules.Notifications;
using Api.Modules.Users;
using Api.Modules.Authorizations;
using Api.Main;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // All endpoints require authentication
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _repository;
        private readonly IUserRepository _usersRepository;

        public NotificationController(
            INotificationRepository repository,
            IUserRepository usersRepository)
        {
            _repository = repository;
            _usersRepository = usersRepository;
        }

        /// <summary>
        /// Get all notifications (UserAccess only)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<Notification>>> GetAll()
        {
            try
            {
                var notifications = await _repository.GetAllAsync();
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get notification by ID (UserAccess only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<Notification>> GetById(long id)
        {
            try
            {
                var notification = await _repository.GetByIdAsync(id);

                if (notification == null)
                    return NotFound(new { message = $"Notification with ID {id} not found." });

                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create new notification (UserAccess only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<Notification>> Create([FromBody] Notification notification)
        {
            try
            {
                if (notification.UserId <= 0)
                {
                    return BadRequest(new { message = "userId is required and must be greater than 0." });
                }

                var user = await _usersRepository.GetByIdAsync((int)notification.UserId);
                if (user == null)
                    return BadRequest(new { message = $"User with ID {notification.UserId} does not exist." });

                await _repository.AddAsync(notification);

                return CreatedAtAction(nameof(GetById), new { id = notification.NotificationId }, notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update notification (UserAccess only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<IActionResult> Update(long id, [FromBody] Notification notification)
        {
            try
            {
                if (notification.NotificationId == 0)
                {
                    notification.NotificationId = id;
                }

                if (id != notification.NotificationId)
                    return BadRequest(new { message = "Route id does not match notification_id in request body." });

                var existingNotification = await _repository.GetByIdAsync(id);
                if (existingNotification == null)
                    return NotFound(new { message = $"Notification with ID {id} not found." });

                if (notification.UserId <= 0)
                {
                    return BadRequest(new { message = "userId is required and must be greater than 0." });
                }

                var user = await _usersRepository.GetByIdAsync((int)notification.UserId);
                if (user == null)
                    return BadRequest(new { message = $"User with ID {notification.UserId} does not exist." });

                await _repository.UpdateAsync(notification);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete notification (UserAccess only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var notification = await _repository.GetByIdAsync(id);

                if (notification == null)
                    return NotFound(new { message = $"Notification with ID {id} not found." });

                await _repository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated notifications (UserAccess only)
        /// </summary>
        [HttpGet("paginated")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<PaginationModel<Notification>>> GetPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
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