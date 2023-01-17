using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Json.Schema;

namespace EMS.Library.JSon
{
    public static class JSon
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static JsonSchema GetSchema(string schemaFile)
        {
            var assembly = System.Reflection.Assembly.GetCallingAssembly();
            return GetSchema(assembly, schemaFile);
        }
        public static JsonSchema GetSchema(System.Reflection.Assembly assembly, string schemaFile)
        {           
            Logger.Trace($"looking for resources in {assembly.Location}");
            foreach (var s in assembly.GetManifestResourceNames())
            {
                Logger.Trace($"resource file {s}");
            }
            using Stream? resource = assembly.GetManifestResourceStream(schemaFile);
            if (resource == null) throw new FileNotFoundException($"Unable to load embedded resource {schemaFile}");
            using var r = new StreamReader(resource);
            var mySchema = JsonSchema.FromText(r.ReadToEnd());
            return mySchema;
        }

        public static void ShowEvaludationDetails(IReadOnlyList<EvaluationResults> results){
            Console.WriteLine(results.Count());            
            var invalids = results.Where((x) => !x.IsValid);
            Console.WriteLine(invalids.Count());
            foreach(var invalid in invalids)
            {
                if (invalid.Details.Count > 0)
                    ShowEvaludationDetails(invalid.Details);
                else {
                    Logger.Error(invalid.SchemaLocation);
                    if (invalid != null && invalid.HasErrors && invalid.Errors != null){
                        foreach(var error in invalid.Errors){
                            Logger.Error(error.Value);
                        }
                    }
                }
            }
        }
    }
}
