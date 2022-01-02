using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace EMS
{
    public static class ConfigurationManager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static JObject ReadConfig(string filename)
        {
            // read JSON directly from a file
            using var streamReader = GetConfigFile(filename);
            using var jsonTextReader = new JsonTextReader(streamReader);
            JObject o2 = (JObject)JToken.ReadFrom(jsonTextReader);
            if (!o2.IsValid(GetSchema(), out IList<string> messages))
            {
                Console.WriteLine(messages.Count);
                foreach (var message in messages)
                {
                    Logger.Error(message);

                }
                throw new ArgumentException("The configuration file has not a valid format.", nameof(filename));
            }
            return o2;
        }

        public static bool ValidateConfig(string filename)
        {
            try
            {
                return ReadConfig(filename) != null;
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

        private static JSchema GetSchema()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Logger.Trace($"looking for resources in {assembly.Location}");
            foreach (var s in assembly.GetManifestResourceNames())
            {
                Logger.Trace($"resource file {s}");
            }
            using Stream resource = assembly.GetManifestResourceStream("EMS.config.schema.json");
            using var r = new StreamReader(resource);
            var schema = r.ReadToEnd();            
            return JSchema.Parse(schema);
        }
    }
}
