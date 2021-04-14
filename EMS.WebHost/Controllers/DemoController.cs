using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMS.Library;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Docs.Samples;
using Microsoft.Extensions.Logging;

namespace EMS.WebHosts
{
    [ApiController]
    [Route("api/[controller]")]
    public class MyDemoController : ControllerBase
    {
        private readonly ILogger _logger;
        private ILogger Logger => _logger;

        private readonly IChargePoint _chargePoint;
        private IChargePoint ChargePoint => _chargePoint;

        public MyDemoController(ILogger<MyDemoController> logger, IChargePoint chargePoint)
        {
            _logger = logger;
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
            Logger.LogInformation($"got request with {id}");
            return $"id ...{id}";
        }
    }
}
