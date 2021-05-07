using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMS.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Docs.Samples;
using Microsoft.Extensions.Logging;

namespace EMS.WebHosts
{

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private ILogger Logger { get; init; }

        public SettingsController(ILogger<MyDemoController> logger)
        {
            Logger = logger;
        }


        [HttpGet]
        public ActionResult<SettingsModel> Get()
        {
            return new SettingsModel();
        }

        [HttpPost]
        public ActionResult<bool> Post([FromBody] SettingsModel model)
        {
            return true;
        }
    }

    public class SettingsModel
    {
        public string theme { get; set; }
    }
}
