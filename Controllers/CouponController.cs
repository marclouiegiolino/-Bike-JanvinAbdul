using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Modules.Coupons;
using Api.Modules.Authorizations;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CouponController : ControllerBase
    {
        private readonly ICouponRepository _repository;

        public CouponController(ICouponRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<IEnumerable<Coupon>>> GetAll()
        {
            return Ok(await _repository.GetAllAsync());
        }

        [HttpGet("{id:long}")]
        [Authorize(Policy = AppPolicies.AllAuthenticatedUsers)]
        public async Task<ActionResult<Coupon>> GetById(long id)
        {
            var coupon = await _repository.GetByIdAsync(id);
            if (coupon is null)
                return NotFound();

            return Ok(coupon);
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
        public async Task<ActionResult<Coupon>> Create(Coupon coupon)
        {
            await _repository.AddAsync(coupon);
            return CreatedAtAction(nameof(GetById), new { id = coupon.CouponId }, coupon);
        }

        [HttpPut("{id:long}")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Update(long id, Coupon coupon)
        {
            if (coupon.CouponId == 0)
                coupon.CouponId = id;

            if (id != coupon.CouponId)
                return BadRequest("Route id does not match category_id in request body.");

            var existing = await _repository.GetByIdAsync(id);
            if (existing is null)
                return NotFound();

            await _repository.UpdateAsync(coupon);
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
