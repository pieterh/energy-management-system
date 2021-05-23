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

namespace EMS.WebHosts
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class HEMSController : ControllerBase                                    //NOSONAR
    {
        private ILogger Logger { get; init; }
        private IHEMSCore Hems { get; init; }

        public HEMSController(ILogger<HEMSController> logger, IHEMSCore hems)
        {
            Logger = logger;
            Hems = hems;
        }

        [Route("api/[controller]/info")]
        public ActionResult<SessionInfoModel> GetHemsInfo()
        {
            var retval = new HemsInfoModel();
            var t = Hems.ChargeControlInfo;
            retval.Mode = t.Mode.ToString();
            retval.State = t.State.ToString();
            retval.LastStateChange = t.LastStateChange;
            retval.CurrentAvailableL1Formatted = PrepareDouble(t.CurrentAvailableL1, 1, "A");
            retval.CurrentAvailableL2Formatted = PrepareDouble(t.CurrentAvailableL1, 1, "A");
            retval.CurrentAvailableL3Formatted = PrepareDouble(t.CurrentAvailableL1, 1, "A");
            return new JsonResult(new HemsInfoResponse() { Info = retval });
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
}
