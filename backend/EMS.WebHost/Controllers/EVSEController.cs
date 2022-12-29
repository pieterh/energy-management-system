using System;
using System.Globalization;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

using EMS.Library;
using EMS.Library.Adapter.EVSE;
using EMS.WebHost.Controllers;
using EMS.WebHost.Helpers;

namespace EMS.WebHosts
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class EVSEController : ControllerBase                                    //NOSONAR
    {
        private ILogger Logger { get; init; }
        private IChargePoint ChargePoint { get; init; }

        public EVSEController(ILogger<EVSEController> logger, IChargePoint chargePoint)
        {
            Logger = logger;
            ChargePoint = chargePoint;
        }

        //[Route("api/[controller]/station")]
        [HttpGet("station")]
        public ActionResult<SessionInfoModel> GetStationInfo()
        {
            var retval = new SessionInfoModel();
            //retval.Mode3State = ChargePoint.LastSocketMeasurement.Mode3State.ToString();
            //retval.VehicleIsConnected = ChargePoint.LastSocketMeasurement.VehicleConnected;
            //retval.VehicleIsCharging = ChargePoint.LastSocketMeasurement.VehicleIsCharging;
            return new JsonResult(retval);
        }
 

        //[Route("api/[controller]/socket/{id}")]
        [HttpGet("socket/{id}")]
        public ActionResult<SocketInfoResponse> GetSocketInfo(int id)
        {
            var socket = new SocketInfoModel();
            SocketMeasurementBase sm = ChargePoint.LastSocketMeasurement;
            socket.Id = id;
            socket.VoltageFormatted = PrepareDouble(sm.Voltage, 1, "V");
            socket.CurrentFormatted = PrepareDouble(sm.CurrentSum, 1, "A");
            socket.RealPowerSumFormatted = PrepareDouble(sm.RealPowerSum / 1000, 1, "kW");                 // kW
            socket.RealEnergyDeliveredFormatted = PrepareDouble(sm.RealEnergyDeliveredSum / 1000, 0, "kW");   // kW
            socket.Availability = sm.Availability;

            socket.Mode3State = sm.Mode3State.ToString();
            socket.Mode3StateMessage = sm.Mode3StateMessage;
            socket.LastChargingStateChanged = sm.LastChargingStateChanged;
            socket.VehicleIsConnected = sm.VehicleConnected;
            socket.VehicleIsCharging = sm.VehicleIsCharging;

            socket.AppliedMaxCurrent = sm.AppliedMaxCurrent;
            socket.MaxCurrentValidTime = sm.MaxCurrentValidTime;

            socket.MaxCurrent = sm.MaxCurrent;
            socket.ActiveLBSafeCurrent = sm.ActiveLBSafeCurrent;
            socket.SetPointAccountedFor = sm.SetPointAccountedFor;
            socket.Phases = sm.Phases;

            socket.PowerAvailableFormatted = PrepareDouble(((sm.Phases == Phases.One ? 1 : 3) * sm.Voltage * (socket.VehicleIsCharging ? socket.AppliedMaxCurrent : socket.MaxCurrent)) / 1000, 1, "kW"); // kW
            socket.PowerUsingFormatted = PrepareDouble((sm.Voltage * sm.CurrentSum) / 1000, 1, "kW"); // kW

            SessionInfoModel session = null;
            if (socket.VehicleIsConnected && ChargePoint.ChargeSessionInfo != null)
            {
                var csi = ChargePoint.ChargeSessionInfo;
                if (csi != null && csi.Start.HasValue)
                {
                    session = new SessionInfoModel();
                    session.Start = csi.Start.Value;
                    session.ChargingTime = csi.ChargingTime;
                    session.EnergyDeliveredFormatted = PrepareDouble(csi.EnergyDelivered / 1000, 1, "kWh"); // kWh
                }
            }

            return new JsonResult(new SocketInfoResponse() { Status = 200, SocketInfo = socket, SessionInfo = session });
        }

        private static string PrepareDouble(double f, int digits, string unitOfMeasurement)
        {
            float retval = (float)Math.Round(f, 1);
            retval = (retval < 0.01) ? 0.0f : retval;
            return string.Format(new NumberFormatInfo() { NumberDecimalDigits = digits }, "{0:F} {1}", retval, unitOfMeasurement);
        }
    }

    public class SocketInfoResponse : Response {
        public SocketInfoModel SocketInfo { get; set; }
        public SessionInfoModel SessionInfo { get; set; }
    }

    public class SocketInfoModel
    {
        public int Id { get; set; }

        public string VoltageFormatted { get; set; }
        public string CurrentFormatted { get; set; }

        public string RealPowerSumFormatted { get; set; }                 // kW
        public string RealEnergyDeliveredFormatted { get; set; }          // kWh


        public bool Availability { get; set; }
        public string Mode3State { get; set; }
        public string Mode3StateMessage { get; set; }
        public DateTime LastChargingStateChanged { get; set; }
        public bool VehicleIsConnected { get; set; }
        public bool VehicleIsCharging { get; set; }

        [JsonConverter(typeof(FloatConverterP1))]
        public float AppliedMaxCurrent { get; set; }
        public UInt32 MaxCurrentValidTime { get; set; }
        [JsonConverter(typeof(FloatConverterP1))]
        public float MaxCurrent { get; set; }

        [JsonConverter(typeof(FloatConverterP0))]
        public float ActiveLBSafeCurrent { get; set; }
        public bool SetPointAccountedFor { get; set; }
        public Phases Phases { get; set; }

        public string PowerAvailableFormatted { get; set; }                 // kW
        public string PowerUsingFormatted { get; set; }                     // kW
    }

    public class SessionInfoModel
    {
        public DateTime Start { get; set; }
        public UInt32 ChargingTime { get; set; }
        public string EnergyDeliveredFormatted { get; set; }                // kWh
    }
}
