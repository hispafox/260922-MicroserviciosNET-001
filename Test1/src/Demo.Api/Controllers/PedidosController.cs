using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MassTransit;
using Orders.Application;
using Integration.Contracts;
using Asp.Versioning;
using Demo.Api; // Importa ApiResponse

namespace Demo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/pedidos")]
[ApiVersion("1.0")]
public class PedidosController : ControllerBase
{
    private readonly OrdersService _svc;
    private readonly IPublishEndpoint _publish;
    public PedidosController(OrdersService svc, IPublishEndpoint publish) { _svc = svc; _publish = publish; }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateOrder cmd, CancellationToken ct)
    {
        var id = await _svc.CreateAsync(cmd, ct);
        await _publish.Publish(new PedidoCreado(id, cmd.Lines.Sum(x => x.Price * x.Quantity)), ct);
        var response = ApiResponse<object>.SuccessResult(null, "Pedido creado correctamente.");
        return CreatedAtAction(nameof(GetById), new { id, version = "1.0" }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var dto = await _svc.GetByIdAsync(id, ct);
        if (dto is null)
        {
            var problem = new ProblemDetails
            {
                Title = "Pedido no encontrado",
                Status = StatusCodes.Status404NotFound,
                Detail = $"No existe un pedido con id {id}."
            };
            return NotFound(problem);
        }
        var response = ApiResponse<object>.SuccessResult(dto);
        return Ok(response);
    }
}
