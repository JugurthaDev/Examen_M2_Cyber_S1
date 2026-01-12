using Examen_Cyber_M2_S1.Services;

using Microsoft.AspNetCore.Mvc;

namespace Examen_Cyber_M2_S1;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IStockService _stockService;

    public ProductsController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_stockService.GetAllProducts());
    }

    [HttpGet("/products")]
    public IActionResult GetAllLegacyFormat()
    {
        var payload = _stockService.GetAllProducts().Select(p => new
        {
            id = p.Id,
            name = p.Name,
            price = p.UnitPrice,
            stock = p.Stock
        });

        return Ok(payload);
    }
}