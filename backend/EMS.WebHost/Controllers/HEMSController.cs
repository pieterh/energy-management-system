using System;
using System.Globalization;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

using EMS.Library;
using EMS.WebHost.Controllers;
using EMS.WebHost.Helpers;
using EMS.Library.Core;
using System.Collections.Generic;
using EMS.DataStore;
using System.Linq;
using EMS.Library.Adapter.PriceProvider;
using System.Diagnostics.CodeAnalysis;

namespace EMS.WebHosts;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[SuppressMessage("SonarLint", "S101", Justification = "Ignored intentionally")]
public class HEMSController : ControllerBase
{
    private ILogger Logger { get; init; }
    private IHEMSCore Hems { get; init; }

    public HEMSController(ILogger<HEMSController> logger, IHEMSCore hems, IPriceProvider priceProvider)
    {
        Logger = logger;
        Hems = hems;
    }

    [Route("api/[controller]/info")]
    [HttpGet]
    [Produces("application/json")]
    public ActionResult<SessionInfoModel> GetHemsInfo()
    {
        var t = Hems.ChargeControlInfo;

        var info = new HemsInfoModel() {
            Mode = t.Mode.ToString(),
            State = t.State.ToString(),
            LastStateChange = t.LastStateChange,
            CurrentAvailableL1Formatted = PrepareDouble(t.CurrentAvailableL1, 1, "A"),
            CurrentAvailableL2Formatted = PrepareDouble(t.CurrentAvailableL1, 1, "A"),
            CurrentAvailableL3Formatted = PrepareDouble(t.CurrentAvailableL1, 1, "A")
        };
        
        var measurements = new List<Measurement>();

        foreach (var m in t.Measurements)
        {
            measurements.Add(new Measurement()
            {
                Received = m.Received,
                L1 = m.L1,
                L2 = m.L2,
                L3 = m.L3,
                CL1 = m.CL1,
                CL2 = m.CL2,
                CL3 = m.CL3,
            });
        }

        return new JsonResult(new HemsInfoResponse() { Status = 200, StatusText = "OK",  Info = info, Measurements = measurements });
    }

    [Route("api/[controller]/sessions")]
    [HttpGet]
    [Produces("application/json")]
    public ActionResult<HemsLastSessionsResponse> GetSessionInfo()
    {
        var sessions = new List<ChargingSession>();

        using (var db = new HEMSContext())
        {

            Logger.LogInformation("Database path: {Path}.", HEMSContext.DbPath);

            var items = db.ChargingTransactions.OrderByDescending((x) => x.Timestamp);
            foreach (var item in items)
            {
                Logger.LogInformation("{Item}", item.ToString());
                var session = new ChargingSession()
                {
                    Timestamp = item.Timestamp,
                    EnergyDelivered = (decimal)item.EnergyDelivered,
                    Price = (decimal)item.Price,
                    Cost = (decimal)item.Cost
                };

                sessions.Add(session);
            }
        }

        return new JsonResult(new HemsLastSessionsResponse() { Status = 200, StatusText = "OK", Sessions = sessions });
    }

    private static string PrepareDouble(double f, int digits, string unitOfMeasurement)
    {
        float retval = (float)Math.Round(f, 1);
        retval = (retval < 0.01) ? 0.0f : retval;
        return string.Format(new NumberFormatInfo() { NumberDecimalDigits = digits }, "{0:F} {1}", retval, unitOfMeasurement);
    }
}

public class HemsInfoResponse : Response
{
    public required HemsInfoModel Info { get; init; }
    public required IEnumerable<Measurement> Measurements { get; init; }
}

public class HemsInfoModel
{
    public required string Mode { get; init; }
    public required string State { get; init; }
    public required DateTime LastStateChange { get; init; }
    public required string CurrentAvailableL1Formatted { get; init; }
    public required string CurrentAvailableL2Formatted { get; init; }
    public required string CurrentAvailableL3Formatted { get; init; }
}

public class Measurement
{
    public DateTime Received { get; init; }
    public double L1 { get; init; }
    public double L2 { get; init; }
    public double L3 { get; init; }
    public double L { get; init; }

    public double CL1 { get; init; }
    public double CL2 { get; init; }
    public double CL3 { get; init; }
}

public class HemsLastSessionsResponse : Response
{
    public required IEnumerable<ChargingSession> Sessions { get; init; }
}

public class ChargingSession
{
    public required DateTime Timestamp { get; init; }
    public required decimal EnergyDelivered { get; init; }
    public required decimal Cost { get; init; }
    public required decimal Price { get; init; }
}