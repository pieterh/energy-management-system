﻿using System;
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
    public class SmartMeterController : ControllerBase
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
            var t = SmartMeter.LastMeasurement;
            if (t == null)
                return new JsonResult(new Response() { Status = 204, StatusText = "No measurement available"});
            var retval = new SmartMeterInfoModel()
            {
                CurrentL1 = PrepareDouble(t.CurrentL1, 1, "A"),
                CurrentL2 = PrepareDouble(t.CurrentL2, 1, "A"),
                CurrentL3 = PrepareDouble(t.CurrentL3, 1, "A"),
                VoltageL1 = PrepareDouble(t.VoltageL1, 1, "V"),
                VoltageL2 = PrepareDouble(t.VoltageL2, 1, "V"),
                VoltageL3 = PrepareDouble(t.VoltageL3, 1, "V"),
                TariffIndicator = t.TariffIndicator ?? 0,
                Electricity1FromGrid = PrepareDouble(t.Electricity1FromGrid, 0, "kWh"),
                Electricity1ToGrid = PrepareDouble(t.Electricity1ToGrid, 0, "kWh"),
                Electricity2FromGrid = PrepareDouble(t.Electricity2FromGrid, 0, "kWh"),
                Electricity2ToGrid = PrepareDouble(t.Electricity2ToGrid, 0, "kWh")
            };

            return new JsonResult(new SmartMeterInfoResponse() { Status = 200, StatusText = "OK", Info = retval });
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
        public required SmartMeterInfoModel Info { get; init; }
    }

    public class SmartMeterInfoModel
    {
        public required string CurrentL1 { get; init; }
        public required string CurrentL2 { get; init; }
        public required string CurrentL3 { get; init; }

        public required string VoltageL1 { get; init; }
        public required string VoltageL2 { get; init; }
        public required string VoltageL3 { get; init; }

        public required int TariffIndicator { get; init; }

        public required string Electricity1FromGrid { get; init; }
        public required string Electricity1ToGrid { get; init; }

        public required string Electricity2FromGrid { get; init; }
        public required string Electricity2ToGrid { get; init; }
    }
}
