using System;
using System.Reflection;
using System.Runtime.InteropServices;
using NLog;

namespace EMS.Library.dotNET
{
	public static class DotNetInfo
	{
		public static void Info(ILogger logger)
		{
            ArgumentNullException.ThrowIfNull(logger);
            logger.Info("System version (CLR's version): {SystemVersion}", RuntimeEnvironment.GetSystemVersion());
            // The following is a bit more detailed then the information from the API -> RuntimeInformation.FrameworkDescription
            logger.Info("CoreCLR Build: {Build}", ((AssemblyInformationalVersionAttribute[])typeof(object).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false))[0].InformationalVersion);
            // The following is a bit more detailed then the information from the API -> Environment.Version
            logger.Info("CoreFX Build: {Build}", ((AssemblyInformationalVersionAttribute[])typeof(Uri).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false))[0].InformationalVersion);

            logger.Info("OSArchitecture: {OSArchitecture}", RuntimeInformation.OSArchitecture);
            logger.Info("OSDescription: {OSDescription}", RuntimeInformation.OSDescription);
            logger.Info("ProcessArchitecture: {ProcessArchitecture}", RuntimeInformation.ProcessArchitecture);
            logger.Info("RuntimeIdentifier: {RuntimeIdentifier}", RuntimeInformation.RuntimeIdentifier);
            logger.Info("Runtime directory: {RuntimeDirectory}", RuntimeEnvironment.GetRuntimeDirectory());
        }

        public static void GCInfo(ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            var gc = GC.GetConfigurationVariables();
            foreach (var i in gc.OrderBy((x) => x.Key))
            {
                logger.Info("{Key}:{Value}", i.Key.Replace("\"", "", StringComparison.Ordinal), i.Value);
            }
        }
	}
}

