using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMS.Library;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace EMS.WebHosts
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class MyDemoController : ControllerBase
    {
        private ILogger Logger { get; init; }        
        private IChargePoint ChargePoint { get; init;  }

        public MyDemoController(ILogger<MyDemoController> logger, IChargePoint chargePoint)
        {
            Logger = logger;
            ChargePoint = chargePoint;
        }        

        [HttpGet]
        public ActionResult<string> Get([FromQuery][BindRequired] int id)
        {
            //http://127.0.0.1:5000/api/MyDemo?id=12
            Logger.LogInformation($"got request with {id} - {ChargePoint.LastSocketMeasurement.Mode3StateMessage}");
            return $"id ...{id}{Environment.NewLine}{ChargePoint.LastSocketMeasurement.Mode3StateMessage}";
        }
    }
}
