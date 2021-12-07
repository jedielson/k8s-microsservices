using Microsoft.AspNetCore.Mvc;

namespace Autorizacao.Controllers;

[ApiController]
[Route("hc")]
public class HealhChecksController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public HealhChecksController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("alive")]
    public IActionResult Alive()
    {
        return Ok(new
        {
            service = "Catalogo",
            status = "Alive",
            version = _configuration.GetValue<string>("AppVersion")
        });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok();
    }
}
