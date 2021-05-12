using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMS.Library;
using EMS.Library.Adapter.EVSE;
using EMS.WebHost.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace EMS.WebHosts
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EVSEController : ControllerBase
    {
        private ILogger Logger { get; init; }
        private IChargePoint ChargePoint { get; init; }

        public EVSEController(ILogger<MyDemoController> logger, IChargePoint chargePoint)
        {
            Logger = logger;
            ChargePoint = chargePoint;
        }

        [Route("api/[controller]/station")]
        public ActionResult<SessionInfoModel> GetStationInfo()
        {
            var retval = new SessionInfoModel();
            retval.Mode3State = ChargePoint.LastSocketMeasurement.Mode3State.ToString();
            retval.VehicleIsConnected = ChargePoint.LastSocketMeasurement.VehicleConnected;
            retval.VehicleIsCharging = ChargePoint.LastSocketMeasurement.VehicleIsCharging;
            return retval;
        }


        [Route("api/[controller]/socket/{id}/session")]
        public ActionResult<SessionInfoResponse> GetSessionInfo(int id)
        {
            var retval = new SessionInfoModel();
            SocketMeasurementBase sm = ChargePoint.LastSocketMeasurement;

            retval.Mode3State = sm.Mode3State.ToString();
            retval.Mode3StateMessage = sm.Mode3StateMessage;
            retval.LastChargingStateChanged = sm.LastChargingStateChanged;

            retval.VehicleIsConnected = sm.VehicleConnected;
            retval.VehicleIsCharging = sm.VehicleIsCharging;
            retval.Phases = sm.Phases;
            retval.AppliedMaxCurrent = sm.AppliedMaxCurrent;
            retval.MaxCurrent = sm.MaxCurrent;

            return new SessionInfoResponse() { Status = 200, SessionInfo = retval };
        }
    }

    public class SessionInfoResponse : Response {
        public SessionInfoModel SessionInfo { get; set; }
    }

    public class SessionInfoModel
    {
        public int id {get;set;}
        public string Mode3State { get; set; }
        public string Mode3StateMessage { get; set; }
        public DateTime LastChargingStateChanged { get; set; }
        public bool VehicleIsConnected { get; set; }
        public bool VehicleIsCharging { get; set; }
        public Phases Phases { get; set; }
        public float AppliedMaxCurrent { get; set; }
        public float MaxCurrent { get; set; }
    }
}
