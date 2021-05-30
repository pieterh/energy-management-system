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
            var lst = new List<Measurement>();
            retval.Measurements = lst;
            foreach (var m in t.Measurements){
                lst.Add(new Measurement() {
                    Received = m.Received,
                    L1 = m.L1,
                    L2 = m.L2,
                    L3 = m.L3,
                    CL1 = m.CL1,
                    CL2 = m.CL2,
                    CL3 = m.CL3,
                });
            }
            
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
        public IEnumerable<Measurement> Measurements  { get; set; }
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

}
