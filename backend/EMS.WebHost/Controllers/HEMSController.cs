using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

using EMS.DataStore;
using EMS.Library;
using EMS.Library.Core;
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.Shared.DTO.HEMS;
using EMS.Library.Shared.DTO.EVSE;
using EMS.WebHost.Controllers;
using EMS.WebHost.Helpers;

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
        
        var measurements = new List<Library.Shared.DTO.HEMS.Measurement>();

        foreach (var m in t.Measurements)
        {
            measurements.Add(new Library.Shared.DTO.HEMS.Measurement()
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

            var items = db.ChargingTransactions.OrderByDescending((x) => x.ID);
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
