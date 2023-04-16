using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Json.Schema;

namespace EMS.Library.JSon
{
    public static class JsonHelpers
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

        public static void ShowEvaluationDetails(IReadOnlyList<EvaluationResults> results)
        {
            ArgumentNullException.ThrowIfNull(results);

            results.Where((x) => !x.IsValid)
                .AsParallel()
                .ForAll((invalid) =>
                    {
                        if (invalid.Details.Count > 0)
                            ShowEvaluationDetails(invalid.Details);
                        if (invalid.HasErrors && invalid.Errors != null)
                        {
                            Logger.Error(invalid.InstanceLocation);
                            foreach (var error in invalid.Errors)
                            {
                                Logger.Error(error.Value);
                            }
                        }                       
                    });
        }
    }
}
