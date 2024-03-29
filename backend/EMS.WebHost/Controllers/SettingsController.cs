﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMS.Library;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EMS.WebHosts;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private ILogger Logger { get; init; }

    public SettingsController(ILogger<SettingsController> logger)
    {
        Logger = logger;
    }


    [HttpGet]
    [Produces("application/json")]
    public ActionResult<SettingsModel> Get()
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Consumes("application/json")]
    [Produces("application/json")]
    public ActionResult<bool> Post([FromBody] SettingsModel model)
    {
        return true;
    }
}

public class SettingsModel
{
    public required string theme { get; init; }
}
