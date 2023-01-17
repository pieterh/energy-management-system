using System;
using System.IO;
using System.Text.Json;
using Json.Schema;
using EMS.Library.JSon;

namespace EMS
{
    public static class ConfigurationManager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions();

        public static JsonElement ReadConfig(string filename)
        {
            // read JSON directly from a file
            using var streamReader = GetConfigFile(filename);
            var result = JsonSerializer.Deserialize<JsonElement>(streamReader.BaseStream, Options);
            var schema = JSon.GetSchema("EMS.config.schema.json");
            SchemaRegistry.Global.Register(new Uri("https://github.com/pieterh/energy-management-system"), schema);
            var validationResult = schema.Evaluate(result, new EvaluationOptions() { OutputFormat = OutputFormat.Hierarchical });
            
            if (!validationResult.IsValid){
                JSon.ShowEvaludationDetails(validationResult.Details);
                throw new ArgumentException("The configuration file has not a valid format.", nameof(filename));
            }
            return result;
        }

        public static bool ValidateConfig(string filename)
        {
            try
            {
                var r = ReadConfig(filename);                
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e, "There was an error validating the config file.");
            }
            return false;
        }
        private static StreamReader GetConfigFile(string cfgn)
        {
            Logger.Trace($"Loading configuration file from {cfgn}");
            return File.OpenText(cfgn);
        }

    }
}
