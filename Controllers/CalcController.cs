using Microsoft.AspNetCore.Mvc;

namespace net6_api.Controllers;

[ApiController]
[Route("[controller]")]
public class CalcController : ControllerBase
{

    private readonly ILogger<CalcController> _logger;

    public CalcController(ILogger<CalcController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Execute([FromServices] Calc calc, decimal x, decimal y, Operation op)
    {        
        return Ok(calc.Calculate(x, y, op));
    }    
}

