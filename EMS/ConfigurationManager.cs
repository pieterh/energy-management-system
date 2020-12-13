using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace EMS
{
    public class ConfigurationManager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public JObject ReadConfig()
        {
            // read JSON directly from a file
            using (StreamReader file = GetConfigFile())
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject o2 = (JObject)JToken.ReadFrom(reader);
                IList<string> messages;
                if (!o2.IsValid(GetSchema(), out messages)){
                    Console.WriteLine(messages.Count);
                    foreach(var message  in messages){
                        Logger.Error(message);
                        
                    }
                    throw new ApplicationException("The configuration file has not a valid format.");
                }
                return o2;
            }
        }

        private static StreamReader GetConfigFile()
        {
            var cb = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var dir = System.IO.Path.GetDirectoryName(cb);
            var cfgn = Path.Combine(dir, "config.json");
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
            Stream resource = assembly.GetManifestResourceStream("ems.config.schema.json");
            var r = new StreamReader(resource);
            var schema = r.ReadToEnd();
            return JSchema.Parse(schema);
        }
    }
}
