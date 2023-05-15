using System;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

using EMS.Library.Shared.DTO;
using EMS.Library.Shared.DTO.Health;
using EMS.Library.Shared.DTO.Users;
using EMS.WebHost.Helpers;

namespace EMS.WebHost.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private ILogger Logger { get; init; }
    private IWebHost WebHost { get; init; }

    public HealthController(ILogger<HealthController> logger, IWebHost webHost)
    {
        Logger = logger;
        WebHost = webHost;
    }

    [AllowAnonymous]
    [HttpGet("check")]
    [ProducesResponseType(typeof(Response), 500, "application/json")]
    [ProducesResponseType(typeof(HealthResponse), 200, "application/json")]    
    public IActionResult HealthCheck()
    {
        var response = new HealthResponse(WebHost.UpSince);
        return Ok(response);
    }
}

