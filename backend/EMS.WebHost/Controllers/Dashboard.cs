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
    public class DashboardController : ControllerBase
    {
        private ILogger Logger { get; init; }
        private IChargePoint ChargePoint { get; init; }

        public DashboardController(ILogger<MyDemoController> logger, IChargePoint chargePoint)
        {
            Logger = logger;
            ChargePoint = chargePoint;
        }

        [HttpGet]
        public ActionResult<DashboardModel> Get()
        {
            var retval = new DashboardModel();
            retval.Mode3State = ChargePoint.LastSocketMeasurement.Mode3State.ToString();
            retval.VehicleIsConnected = ChargePoint.LastSocketMeasurement.VehicleConnected;
            retval.VehicleIsCharging = ChargePoint.LastSocketMeasurement.VehicleIsCharging;
            return retval;
        }
    }

    public class DashboardModel
    {
        public string Mode3State { get; set; }
        public bool VehicleIsConnected { get; set; }
        public bool VehicleIsCharging { get; set; }
    }
}
