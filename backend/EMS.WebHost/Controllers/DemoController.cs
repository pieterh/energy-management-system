using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMS.Library;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Docs.Samples;
using Microsoft.Extensions.Logging;

namespace EMS.WebHosts
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class MyDemoController : ControllerBase
    {
        private ILogger Logger { get; init; }

        private readonly IChargePoint _chargePoint;
        private IChargePoint ChargePoint => _chargePoint;

        public MyDemoController(ILogger<MyDemoController> logger, IChargePoint chargePoint)
        {
            Logger = logger;
            _chargePoint = chargePoint;
        }        

        //[HttpGet]
        //public ActionResult<string> GetAll()
        //{            
        //    return $"uh... {Environment.NewLine}{ChargePoint.LastSocketMeasurement?.ToString()}...";
        //}

        [HttpGet]
        public ActionResult<string> Get([FromQuery][BindRequired] int id)
        {
            //http://127.0.0.1:5000/api/MyDemo?id=12
            Logger.LogInformation($"got request with {id}");
            return $"id ...{id}";
        }
    }
}
