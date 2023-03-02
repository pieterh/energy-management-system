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
            var mySchema = JsonSchema.FromText(ResourceHelper.ReadAsString(assembly, schemaFile));
            return mySchema;
        }

        public static void ShowEvaluationDetails(IReadOnlyList<EvaluationResults> results) {
            if (results == null) throw new ArgumentNullException(nameof(results));
#if DEBUG
            Console.WriteLine(results.Count);
#endif
            results.Where((x) => !x.IsValid)
                .AsParallel()
                .ForAll((invalid) =>
                    {
                        if (invalid.Details.Count > 0)
                            ShowEvaluationDetails(invalid.Details);
                        else
                        {
                            Logger.Error(invalid.SchemaLocation);
                            if (invalid.HasErrors && invalid.Errors != null)
                            {
                                foreach (var error in invalid.Errors)
                                {
                                    Logger.Error(error.Value);
                                }
                            }
                        }
                    } );
        }
    }
}
