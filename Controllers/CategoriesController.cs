using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Modules.Categories;
using Api.Modules.Authorizations;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoriesRepository _repository;

        public CategoriesController(ICategoriesRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<Categories>>> GetAll()
        {
            return Ok(await _repository.GetAllAsync());
        }

        [HttpGet("{id:long}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<Categories>> GetById(long id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category is null)
                return NotFound();

            return Ok(category);
        }

        [HttpGet("paginated")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<IActionResult> GetPaginated(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            return Ok(await _repository.GetPaginatedAsync(pageNumber, pageSize));
        }

        [HttpPost]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<ActionResult<Categories>> Create(Categories category)
        {
            await _repository.AddAsync(category);
            return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, category);
        }

        [HttpPut("{id:long}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Update(long id, Categories category)
        {
            if (category.CategoryId == 0)
                category.CategoryId = id;

            if (id != category.CategoryId)
                return BadRequest("Route id does not match category_id in request body.");

            var existing = await _repository.GetByIdAsync(id);
            if (existing is null)
                return NotFound();

            await _repository.UpdateAsync(category);
            return NoContent();
        }

        [HttpDelete("{id:long}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing is null)
                return NotFound();

            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}
