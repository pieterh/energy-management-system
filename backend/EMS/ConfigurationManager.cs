using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using EMS.Library.Exceptions;
using EMS.Library.JSon;

namespace EMS;

public static class ConfigurationManager
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private static readonly JsonSerializerOptions Options = new();
    private static readonly Uri _schemaUri = new("https://github.com/pieterh/energy-management-system");
    private const string _schemaResourceName = "config.schema.json";

    static ConfigurationManager()
    {
        JsonHelpers.LoadAndRegisterSchema(_schemaUri, _schemaResourceName);
    }

    public static JsonElement ReadConfig(string filename)
    {
        // read JSON directly from a file
        using var streamReader = GetConfigFile(filename);
        var result = JsonSerializer.Deserialize<JsonElement>(streamReader.BaseStream, Options);
        var isValid = JsonHelpers.Evaluate(_schemaResourceName, result);

        if (!isValid)
        {
            Logger.Error("There was an error in the format of the configuration file {FileName}", filename);
            throw new ArgumentException("The configuration file has not a valid format.", nameof(filename));
        }
        return result;
    }

    /// <summary>
    /// Reads a given property from the configuration file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configFile"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    public static T ReadConfigProperty<T>([NotNull] string? configFile, string property)
    {
        NullException.ThrowIfNull(configFile, "No configuration provided");
        using var jd = JsonDocument.Parse(File.ReadAllText(configFile));
        var wce = jd.RootElement.GetProperty(property);
        var wc = wce.Deserialize<T>();
        NullException.ThrowIfNull(wc, $"The {property} configuration is not found");
        return wc;
    }

    public static bool ValidateConfig(string? filename)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filename)) return false;
            var r = ReadConfig(filename);
            return !string.IsNullOrWhiteSpace(r.GetRawText());
        }
        catch (System.Text.Json.JsonException e)
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