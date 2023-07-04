using EMS.Library.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EMS.Library
{
    public static class BackgroundServiceHelper
    {
        public static void CreateAndStart<TInterface, TClass>(IServiceCollection services, params object[] constructorArgs)
                                //where TInterface : IHostedService
                                where TClass : class                                
        { 
            // hack??
            // we want a type created as a hosted service and running but still be able to
            // get a reference to it using the given interface.
            // Is there a better way to do this, then the following two steps?
            services.AddSingleton(typeof(TInterface), x =>
            {
                var s = ActivatorUtilities.CreateInstance(x, typeof(TClass), constructorArgs);
                return s;
            });

            services.AddSingleton<IHostedService>(x =>
            {
                // here we trigger, the createinstance defined in the previous step
                var s = x.GetService(typeof(TInterface)) as IHostedService;
                if (s == null)
                    throw new Exceptions.ApplicationException(string.Format($"Unable to find service with name {typeof(TInterface).FullName} and implementing IHostedService"));
                return s;
            });
        }
    }
}
