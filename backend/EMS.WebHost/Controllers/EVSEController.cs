using System;
using System.Globalization;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

using EMS.Library;
using EMS.Library.Adapter.EVSE;
using EMS.WebHost.Controllers;
using EMS.WebHost.Helpers;
using System.Diagnostics.CodeAnalysis;

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
    public ActionResult<StationInfoModel> GetStationInfo()
    {
        var pis = ChargePoint.ReadProductInformation();
        var sss = ChargePoint.ReadStationStatus();

        var retval = StationInfoModel.Factory(pis, sss);

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
                Phases = sm.Phases,
                PowerAvailableFormatted = PrepareDouble(((sm.Phases == Phases.One ? 1 : 3) * sm.Voltage * (sm.VehicleIsCharging ? sm.AppliedMaxCurrent : sm.MaxCurrent)) / 1000, 1, "kW"),
                PowerUsingFormatted = PrepareDouble((sm.Voltage * sm.CurrentSum) / 1000, 1, "kW")
            };

        var csi = ChargePoint.ChargeSessionInfo;

        SessionInfoModel? session = null;
        if (socket != null && socket.VehicleIsConnected && csi != null && csi.Start.HasValue)
        {
            session = new SessionInfoModel
            {
                Start = csi.Start.Value,
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

public class SocketInfoResponse : Response
{
    public required SocketInfoModel? SocketInfo { get; init; }
    public required SessionInfoModel? SessionInfo { get; init; }
}

public class SocketInfoModel
{
    public int Id { get; set; }

    public required string VoltageFormatted { get; init; }
    public required string CurrentFormatted { get; init; }

    public required string RealPowerSumFormatted { get; init; }                 // kW
    public required string RealEnergyDeliveredFormatted { get; init; }          // kWh


    public required bool Availability { get; init; }
    public required string Mode3State { get; init; }
    public required string Mode3StateMessage { get; init; }
    public required DateTime LastChargingStateChanged { get; init; }
    public required bool VehicleIsConnected { get; init; }
    public required bool VehicleIsCharging { get; init; }

    [JsonConverter(typeof(FloatConverterP1))]
    public required float AppliedMaxCurrent { get; init; }
    public required UInt32 MaxCurrentValidTime { get; init; }
    [JsonConverter(typeof(FloatConverterP1))]
    public required float MaxCurrent { get; init; }

    [JsonConverter(typeof(FloatConverterP0))]
    public required float ActiveLBSafeCurrent { get; init; }
    public required bool SetPointAccountedFor { get; init; }
    public required Phases Phases { get; init; }

    public required string PowerAvailableFormatted { get; init; }                 // kW
    public required string PowerUsingFormatted { get; init; }                     // kW
}

public class SessionInfoModel
{
    public required DateTime Start { get; init; }
    public required UInt32 ChargingTime { get; init; }
    public required string EnergyDeliveredFormatted { get; init; }                // kWh
}

public record StationInfoModel
{
    public static StationInfoModel Factory(ProductInformation productInfo, StationStatus stationStatus)
    {
        return new StationInfoModel()
        {
            ProductInfo = ProductInfoModel.Factory(productInfo),
            StationStatus = StationStatusInfoModel.Factory(stationStatus)
        };
    }

    public required ProductInfoModel ProductInfo { get; init; }
    public required StationStatusInfoModel StationStatus { get; init; }
}

public record ProductInfoModel
{
    public static ProductInfoModel Factory(ProductInformation pi)
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

    public required string Name { get; init; }
    public required string Manufacturer { get; init; }
    public required string FirmwareVersion { get; init; }
    public required string Model { get; init; }
    public required string StationSerial { get; init; }
    public required long Uptime { get; init; }
    public required DateTime DateTimeUtc { get; init; }
}

public record StationStatusInfoModel
{
    public static StationStatusInfoModel Factory(StationStatus ss)
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

    public required float ActiveMaxCurrent { get; init; }
    public required float Temperature { get; init; }
    public required string OCCPState { get; init; }
    public required uint NrOfSockets { get; init; }
}
