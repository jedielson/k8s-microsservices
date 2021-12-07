using Microsoft.AspNetCore.Mvc;

namespace Catalogo.Controllers;

[ApiController]
[Route("catalogo")]
public class CatalogoController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public CatalogoController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            service = "Catalogo",
            status = "Alive",
            version = _configuration.GetValue<string>("AppVersion")
        });
    }
}
