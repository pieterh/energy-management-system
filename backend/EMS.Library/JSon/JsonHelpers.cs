using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Json.Schema;

namespace EMS.Library.JSon;

public static class JsonHelpers
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private static readonly Dictionary<string, JsonSchema> _registeredSchemas = new Dictionary<string, JsonSchema>();

    public static bool LoadAndRegisterSchema(Uri schemaUri, string schemaFile)
    {
        var retval = false;
        var assembly = System.Reflection.Assembly.GetCallingAssembly();
        var schema = JsonHelpers.GetSchema(assembly, schemaFile);
        if (schema != null)
        {
            SchemaRegistry.Global.Register(schemaUri, schema);
            lock (_registeredSchemas)
            {
                _registeredSchemas.Remove(schemaFile);          // can safely remove, even if the key wasn't present in the dictionary.
                _registeredSchemas.Add(schemaFile, schema);
            }
            retval = true;
        }

        return retval;
    }

    public static bool Evaluate(string schemaFile, JsonElement json)
    {
        JsonSchema schema;
        lock (_registeredSchemas)
            schema = _registeredSchemas[schemaFile];

        var validationResult = schema.Evaluate(json, new EvaluationOptions() { OutputFormat = OutputFormat.Hierarchical });
        if (!validationResult.IsValid)
        {
            ShowEvaluationDetails(validationResult.Details);
        }
        return validationResult.IsValid;
    }

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

    /// <summary>
    /// Parses the javascript object and returns all the properties in the dictionary.
    /// </summary>
    /// <param name="javascriptObject"></param>
    /// <returns></returns>
    public static Dictionary<string, string> JavascriptObjectToDictionary(string javascriptObject)
    {
        var jarReg = new Regex("""([^{\n\s,:]*):\s*("([^",]*)"|([^",]*)),?""", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled, new TimeSpan(0, 0, 0, 0, 200));

        var jMatches = jarReg.Match(javascriptObject);
        var dict = new Dictionary<string, string>();
        while (jMatches.Success)
        {
            var propname = jMatches.Groups[1].Value;
            var val = string.IsNullOrWhiteSpace(jMatches.Groups[3].Value) ? jMatches.Groups[2].Value : jMatches.Groups[3].Value;
            Logger.Trace("{propname}:{val}", propname, val);
            dict.Add(propname, val);
            jMatches = jMatches.NextMatch();
        }

        return dict;
    }
}