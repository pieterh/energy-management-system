using System;
using Json.Schema;
using System.IO;

namespace EMS.Library
{
	public static class ResourceHelper
	{
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Reads the resource from the assembly.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourceName">Name of the resource, normally in the form of ({assembly}.{filename})/param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static string ReadAsString(System.Reflection.Assembly assembly, string resourceName)
        {
#if DEBUG
            Logger.Trace($"looking for resources in {assembly.Location}");
            foreach (var s in assembly.GetManifestResourceNames())
            {
                Logger.Trace($"resource file {s}");
            }
#endif
            using Stream? resource = assembly.GetManifestResourceStream(resourceName);
            
            if (resource == null) throw new FileNotFoundException($"Unable to load embedded resource {resourceName}");
            using var r = new StreamReader(resource);
            return r.ReadToEnd();
        }

        public static void LogAllResourcesInAssembly(System.Reflection.Assembly assembly)
        {
            Logger.Info($"Found following resources in assembly {assembly.GetName().FullName}");
            foreach (var r in assembly.GetManifestResourceNames())
            {
                Logger.Info($"resource {r}");
            }
        }
    }
}

