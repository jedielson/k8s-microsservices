using Microsoft.AspNetCore.Mvc;

namespace Autorizacao.Controllers;

[ApiController]
[Route("autorizacao")]
public class AutorizacaoController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AutorizacaoController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IActionResult Get()
    {
        return Ok(new { service = "Autorizacao", status = "Alive", version = _configuration.GetValue<string>("AppVersion") });
    }
}


