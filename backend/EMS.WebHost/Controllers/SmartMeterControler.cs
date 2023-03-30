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
using EMS.Library.Adapter.SmartMeterAdapter;

namespace EMS.WebHosts
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SmartMeterController : ControllerBase                                    //NOSONAR
    {
        private ILogger Logger { get; init; }
        private ISmartMeterAdapter SmartMeter { get; init; }

        public SmartMeterController(ILogger<HEMSController> logger, ISmartMeterAdapter smartMeter)
        {
            Logger = logger;
            SmartMeter = smartMeter;
        }

        [Route("api/[controller]/info")]
        [HttpGet]
        [Produces("application/json")]
        public ActionResult<SessionInfoModel> GetSmartMeterInfo()
        {
            var retval = new SmartMeterInfoModel();
            var t = SmartMeter.LastMeasurement;
            
            retval.CurrentL1 = PrepareDouble(t.CurrentL1, 1, "A");
            retval.CurrentL2 = PrepareDouble(t.CurrentL2, 1, "A");
            retval.CurrentL3 = PrepareDouble(t.CurrentL3, 1, "A");
            retval.VoltageL1 = PrepareDouble(t.VoltageL1, 1, "V");
            retval.VoltageL2 = PrepareDouble(t.VoltageL2, 1, "V");
            retval.VoltageL3 = PrepareDouble(t.VoltageL3, 1, "V");

            retval.TariffIndicator = t.TariffIndicator;
            retval.Electricity1FromGrid = PrepareDouble(t.Electricity1FromGrid, 0, "kWh");
            retval.Electricity1ToGrid = PrepareDouble(t.Electricity1ToGrid, 0, "kWh");
            retval.Electricity2FromGrid = PrepareDouble(t.Electricity2FromGrid, 0, "kWh");
            retval.Electricity2ToGrid = PrepareDouble(t.Electricity2ToGrid, 0, "kWh");

            return new JsonResult(new SmartMeterInfoResponse() { Info = retval });
        }

        private static string PrepareDouble(double? f, int digits, string unitOfMeasurement)
        {
            if (!f.HasValue) return string.Format($"-.- {unitOfMeasurement}");

            float retval = (float)Math.Round(f.Value, 1);
            retval = (retval < 0.01) ? 0.0f : retval;
            return string.Format(new NumberFormatInfo() { NumberDecimalDigits = digits }, "{0:F} {1}", retval, unitOfMeasurement);
        }
    }

    public class SmartMeterInfoResponse : Response
    {
        public SmartMeterInfoModel Info { get; set; }
    }

    public class SmartMeterInfoModel
    {
        public string CurrentL1 { get; set; }
        public string CurrentL2 { get; set; }
        public string CurrentL3 { get; set; }

        public string VoltageL1 { get; set; }
        public string VoltageL2 { get; set; }
        public string VoltageL3 { get; set; }

        public int? TariffIndicator { get;  set; }

        public string Electricity1FromGrid { get;  set; }
        public string Electricity1ToGrid { get; set; }

        public string Electricity2FromGrid { get;  set; }
        public string Electricity2ToGrid { get;  set; }
    }
}
