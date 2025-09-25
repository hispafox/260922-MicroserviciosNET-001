using Microsoft.AspNetCore.Mvc;
using Orders.Api.Clients;

namespace Orders.Api.Controllers
{
    [ApiController]
    [Route("api/v{version}/menu-client")]
    public class MenuClientController : ControllerBase
    {
        private readonly IMenuClient _menuClient;
        public MenuClientController(IMenuClient menuClient)
        {
            _menuClient = menuClient;
        }

        [HttpGet]
        public async Task<ActionResult<List<MenuItemDto>>> GetAll(CancellationToken ct)
        {
            var items = await _menuClient.GetAllAsync(ct);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MenuItemDto>> GetById(int id, CancellationToken ct)
        {
            var item = await _menuClient.GetAsync(id, ct);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateMenuItemDto dto, CancellationToken ct)
        {
            var id = await _menuClient.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id, version = "1.0" }, id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMenuItemDto dto, CancellationToken ct)
        {
            await _menuClient.UpdateAsync(id, dto, ct);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _menuClient.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
