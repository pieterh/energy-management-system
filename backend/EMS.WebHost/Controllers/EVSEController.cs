using System;
using System.Globalization;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

using EMS.Library;
using EMS.Library.Adapter.EVSE;
using EMS.Library.Shared.DTO.EVSE;
using EMS.WebHost.Controllers;
using EMS.WebHost.Helpers;

namespace EMS.WebHosts;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
[SuppressMessage("SonarLint", "S101", Justification = "Ignored intentionally")]
public class EVSEController : ControllerBase
{
    private ILogger Logger { get; init; }
    private IChargePoint ChargePoint { get; init; }

    public EVSEController(ILogger<EVSEController> logger, IChargePoint chargePoint)
    {
        Logger = logger;
        ChargePoint = chargePoint;
    }

    [HttpGet("station")]
    [Produces("application/json")]
    public ActionResult<StationInfoResponse> GetStationInfo()
    {
        var pis = ChargePoint.ReadProductInformation();
        var sss = ChargePoint.ReadStationStatus();
        var retval = new StationInfoResponse()
        {
            Status = 200,
            StatusText = "OK",
            ProductInfo = pis.CreateModel(),
            StationStatus = sss.CreateModel()
        };

        return new JsonResult(retval);
    }

    [HttpGet("socket/{id}")]
    [Produces("application/json")]
    public ActionResult<SocketInfoResponse> GetSocketInfo(int id)
    {
        SocketMeasurementBase? sm = ChargePoint.LastSocketMeasurement;

        SocketInfoModel? socket = null;

        if (sm != null)
            socket = new SocketInfoModel()
            {
                Id = id,
                VoltageFormatted = PrepareDouble(sm.Voltage, 1, "V"),
                CurrentFormatted = PrepareDouble(sm.CurrentSum, 1, "A"),
                RealPowerSumFormatted = PrepareDouble(sm.RealPowerSum / 1000, 1, "kW"),
                RealEnergyDeliveredFormatted = PrepareDouble(sm.RealEnergyDeliveredSum / 1000, 0, "kW"),
                Availability = sm.Availability,
                Mode3State = sm.Mode3State.ToString(),
                Mode3StateMessage = sm.Mode3StateMessage,
                LastChargingStateChanged = sm.LastChargingStateChanged,
                VehicleIsConnected = sm.VehicleConnected,
                VehicleIsCharging = sm.VehicleIsCharging,
                AppliedMaxCurrent = sm.AppliedMaxCurrent,
                MaxCurrentValidTime = sm.MaxCurrentValidTime,
                MaxCurrent = sm.MaxCurrent,
                ActiveLBSafeCurrent = sm.ActiveLBSafeCurrent,
                SetPointAccountedFor = sm.SetPointAccountedFor,
                Phases = sm.Phases.CreateModel(),
                PowerAvailableFormatted = PrepareDouble(((sm.Phases == Library.Adapter.EVSE.Phases.One ? 1 : 3) * sm.Voltage * (sm.VehicleIsCharging ? sm.AppliedMaxCurrent : sm.MaxCurrent)) / 1000, 1, "kW"),
                PowerUsingFormatted = PrepareDouble((sm.Voltage * sm.CurrentSum) / 1000, 1, "kW")
            };

        var csi = ChargePoint.ChargeSessionInfo;

        SessionInfoModel? session = null;
        if (socket != null && socket.VehicleIsConnected && csi != null && csi.Start > DateTimeOffset.MinValue)
        {
            session = new SessionInfoModel
            {
                Start = csi.Start,
                ChargingTime = csi.ChargingTime,
                EnergyDeliveredFormatted = PrepareDouble(csi.EnergyDelivered / 1000, 1, "kWh") // kWh
            };
        }

        return new JsonResult(new SocketInfoResponse() { Status = 200, StatusText = "OK", SocketInfo = socket, SessionInfo = session });
    }

    private static string PrepareDouble(double f, int digits, string unitOfMeasurement)
    {
        float retval = (float)Math.Round(f, 1);
        retval = (retval < 0.01) ? 0.0f : retval;
        return string.Format(new NumberFormatInfo() { NumberDecimalDigits = digits }, "{0:F} {1}", retval, unitOfMeasurement);
    }
}

static class Extensions
{
    public static ProductInfoModel CreateModel(this ProductInformation pi)
    {
        ArgumentNullException.ThrowIfNull(pi);
        return new ProductInfoModel()
        {
            Name = pi.Name,
            Manufacturer = pi.Manufacturer,
            FirmwareVersion = pi.FirmwareVersion,
            Model = pi.Model,
            StationSerial = pi.StationSerial,
            Uptime = pi.Uptime,
            DateTimeUtc = pi.DateTimeUtc
        };
    }

    public static StationStatusInfoModel CreateModel(this StationStatus ss)
    {
        ArgumentNullException.ThrowIfNull(ss);
        return new StationStatusInfoModel()
        {
            ActiveMaxCurrent = ss.ActiveMaxCurrent,
            Temperature = ss.Temperature,
            OCCPState = ss.OCCPState.ToString(),
            NrOfSockets = ss.NrOfSockets
        };
    }

    public static Library.Shared.DTO.EVSE.Phases CreateModel(this Library.Adapter.EVSE.Phases p) => p switch
    {
        Library.Adapter.EVSE.Phases.Unknown => Library.Shared.DTO.EVSE.Phases.Unknown,
        Library.Adapter.EVSE.Phases.One => Library.Shared.DTO.EVSE.Phases.One,
        Library.Adapter.EVSE.Phases.Three => Library.Shared.DTO.EVSE.Phases.One,
        _ => throw new ArgumentOutOfRangeException(nameof(p))
    };
}