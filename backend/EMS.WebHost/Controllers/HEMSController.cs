using System;
using System.Globalization;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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


namespace EMS.WebHosts
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class HEMSController : ControllerBase                                    //NOSONAR
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
            var info = new HemsInfoModel();
            var t = Hems.ChargeControlInfo;
            info.Mode = t.Mode.ToString();
            info.State = t.State.ToString();
            info.LastStateChange = t.LastStateChange;
            info.CurrentAvailableL1Formatted = PrepareDouble(t.CurrentAvailableL1, 1, "A");
            info.CurrentAvailableL2Formatted = PrepareDouble(t.CurrentAvailableL1, 1, "A");
            info.CurrentAvailableL3Formatted = PrepareDouble(t.CurrentAvailableL1, 1, "A");
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

            return new JsonResult(new HemsInfoResponse() { Info = info, Measurements = measurements });
        }

        [Route("api/[controller]/sessions")]
        [HttpGet]
        [Produces("application/json")]
        public ActionResult<HemsLastSessionsResponse> GetSessionInfo()
        {
            var sessions = new List<Session>();

            using (var db = new HEMSContext())
            {

                Logger.LogInformation("Database path: {path}.", HEMSContext.DbPath);

                var items = db.ChargingTransactions.OrderByDescending((x) => x.Timestamp);
                foreach (var item in items)
                {
                    Logger.LogInformation("{item}", item.ToString());
                    var session = new Session()
                    {
                        Timestamp = item.Timestamp,
                        EnergyDelivered = (decimal)item.EnergyDelivered,
                        Price = (decimal)item.Price,
                        Cost = (decimal)item.Cost
                    };

                    sessions.Add(session);
                }
            }

            return new JsonResult(new HemsLastSessionsResponse() { Sessions = sessions });
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
        public HemsInfoModel Info { get; set; }
        public IEnumerable<Measurement> Measurements { get; set; }
    }

    public class HemsInfoModel
    {
        public string Mode { get; set; }
        public string State { get; set; }
        public DateTime LastStateChange { get; set; }
        public string CurrentAvailableL1Formatted { get; set; }
        public string CurrentAvailableL2Formatted { get; set; }
        public string CurrentAvailableL3Formatted { get; set; }
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
        public IEnumerable<Session> Sessions { get; set; }
    }

    public class Session
    {
        public DateTime Timestamp { get; set; }
        public decimal EnergyDelivered { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
    }
}