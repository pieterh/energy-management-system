using System;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace EMS.Library.Assembly
{
	public static class AssemblyInfo
	{
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public static void Init()
		{
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyLoad += new AssemblyLoadEventHandler(MyAssemblyLoadEventHandler);

            var allAppInfo = new StringBuilder();
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (LogAssembly(a))
                {
                    StringBuilder info = GetInfo(a);
                    Logger.Info(info);
                }
            }
        }

		public static StringBuilder GetInfo(System.Reflection.Assembly assembly)
		{
			var str = new StringBuilder();
            str.AppendJoin(' ', "Loaded ==>", assembly.GetName().FullName);
			return str;
		}

        private static void MyAssemblyLoadEventHandler(object? sender, AssemblyLoadEventArgs args)
        {
            if (LogAssembly(args.LoadedAssembly))
                Logger.Info(GetInfo(args.LoadedAssembly));
        }

        private static bool LogAssembly(System.Reflection.Assembly a)
        {
            var name = a.GetName().Name ?? string.Empty;
            return (
                 !name.StartsWith("Microsoft")  &&
                 !name.StartsWith("System") &&
                 !name.StartsWith("Anonymously Hosted DynamicMethods")
            );
        }
    }
}

