using Microsoft.AspNetCore.Mvc;

namespace Assinatura.Controllers;

[ApiController]
[Route("assinatura")]
public class AssinaturaController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AssinaturaController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            service = "Assinatura",
            status = "Alive",
            version = _configuration.GetValue<string>("AppVersion")
        });
    }
}
