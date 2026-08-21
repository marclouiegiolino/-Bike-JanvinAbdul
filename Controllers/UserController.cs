using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Api.Modules.Users;
using Api.Modules.Authorizations;
using Api.DTOs;
using Api.Main;
using Api.Security;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // All endpoints require authentication
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _repository;

        public UserController(IUserRepository repository)
        {
            _repository = repository;
        }

        private static UserResponse ToResponse(User entity)
        {
            return new UserResponse
            {
                UserId = entity.UserId,
                UserName = entity.UserName,
                UserRoleId = (int)entity.RoleId,
            };
        }

        public sealed class SearchByFilterRequest
        {
            public int? userId { get; set; }
        }

        private static Dictionary<string, object?> BuildFilterMap(SearchByFilterRequest request)
        {
            var filters = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            if (request.userId != null)
            {
                filters["userId"] = request.userId;
            }
            return filters;
        }

        /// <summary>
        /// Get all users — Admin only
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                return Ok(result.Select(ToResponse).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get user by ID — all authenticated users (customers view own profile)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<UserResponse>> GetById(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                    return NotFound(new { message = "User not found" });
                return Ok(ToResponse(entity));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Dynamic exact-match filters for users — Admin only
        /// </summary>
        [HttpGet("search-by-filter-ematch")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<ActionResult<IEnumerable<UserResponse>>> SearchByFilterExact([FromQuery] SearchByFilterRequest request)
        {
            try
            {
                IReadOnlyDictionary<string, object?> filters = BuildFilterMap(request);
                if (filters.Count == 0)
                {
                    return BadRequest(new { message = "At least one filter must be provided." });
                }

                var result = await _repository.GetFilteredExactAsync(filters);
                return Ok(result.Select(ToResponse).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Dynamic partial-match filters for users — Admin only
        /// </summary>
        [HttpGet("search-by-filter-lmatch")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<ActionResult<IEnumerable<UserResponse>>> SearchByFilterLike([FromQuery] SearchByFilterRequest request)
        {
            try
            {
                IReadOnlyDictionary<string, object?> filters = BuildFilterMap(request);
                if (filters.Count == 0)
                {
                    return BadRequest(new { message = "At least one filter must be provided." });
                }

                var result = await _repository.GetFilteredLikeAsync(filters);
                return Ok(result.Select(ToResponse).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Search all users — Admin only
        /// </summary>
        [HttpGet("search-all")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<ActionResult<IEnumerable<UserResponse>>> SearchAll([FromQuery] string query)
        {
            try
            {
                var result = await _repository.SearchAsyncAll(query);
                return Ok(result.Select(ToResponse).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated users — Admin only
        /// </summary>
        [HttpGet("paginated")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<ActionResult> GetPaginated(int pageNumber = 1, int pageSize = 25)
        {
            try
            {
                var pagedResult = await _repository.GetPaginatedAsync(pageNumber, pageSize);
                var response = new PaginationModel<UserResponse>
                {
                    Items = pagedResult.Items.Select(ToResponse).ToList(),
                    TotalCount = pagedResult.TotalCount,
                    PageSize = pagedResult.PageSize,
                    CurrentPage = pagedResult.CurrentPage
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create new user/staff account — Admin only
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(request.UserName))
                    return BadRequest(new { message = "UserName is required" });
                if (string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest(new { message = "Password is required" });

                var entity = new User
                {
                    UserName = request.UserName,
                    PasswordHash = PasswordHasher.Hash(request.Password),
                    RoleId = request.UserRoleId,
                };

                await _repository.AddAsync(entity);
                return Ok(new { message = "User created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update user profile — all authenticated users (customers edit own; Admin edits any)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(request.UserName))
                    return BadRequest(new { message = "UserName is required" });

                // Get existing entity
                var existingEntity = await _repository.GetByIdAsync(id);
                if (existingEntity == null)
                    return NotFound(new { message = "User not found" });

                // Update properties
                existingEntity.UserName = request.UserName;
                if (!string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    existingEntity.PasswordHash = PasswordHasher.Hash(request.NewPassword);
                }
                existingEntity.RoleId = request.UserRoleId;

                await _repository.UpdateAsync(existingEntity);
                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete user — Admin only
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Get existing entity
                var existingEntity = await _repository.GetByIdAsync(id);
                if (existingEntity == null)
                    return NotFound(new { message = "User not found" });

                await _repository.DeleteAsync(id);
                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete all users — Admin only
        /// </summary>
        [HttpDelete]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> DeleteAll()
        {
            try
            {
                await _repository.DeleteAllAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Bulk upload users — Admin only
        /// </summary>
        [HttpPost("bulk-upload")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> BulkUpload([FromBody] List<CreateUserRequest> dataList)
        {
            try
            {
                if (dataList == null || dataList.Count == 0)
                    return BadRequest("Data list cannot be null or empty.");

                if (dataList.Any(item => string.IsNullOrWhiteSpace(item.UserName)))
                    return BadRequest(new { message = "One or more records have empty UserName." });
                if (dataList.Any(item => string.IsNullOrWhiteSpace(item.Password)))
                    return BadRequest(new { message = "One or more records have empty Password." });

                var entities = dataList.Select(item => new User
                {
                    UserName = item.UserName,
                    PasswordHash = PasswordHasher.Hash(item.Password),
                    RoleId = item.UserRoleId,
                }).ToList();

                await _repository.BulkUploadAsync(entities);
                return Ok(new { message = $"Successfully uploaded {entities.Count} user records." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}