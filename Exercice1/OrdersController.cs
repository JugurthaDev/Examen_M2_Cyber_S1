using Examen_Cyber_M2_S1.Services;
using Examen_Cyber_M2_S1.Models;
using Examen_Cyber_M2_S1.Services;
using Microsoft.AspNetCore.Mvc;

namespace Examen_Cyber_M2_S1;

[ApiController]
public sealed class OrdersController : ControllerBase
{
    private readonly IStockService _stockService;

    public OrdersController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpPost("/orders")]
    public IActionResult Create([FromBody] CreateOrderRequest request)
    {
        var (response, errors) = OrderCalculator.TryCreateOrder(_stockService, request);

        if (errors.Count > 0)
        {
            return BadRequest(new ErrorResponse { Errors = errors });
        }

        return Ok(response);
    }
}

