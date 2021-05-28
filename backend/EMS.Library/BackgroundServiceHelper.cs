using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EMS.Library
{
    public static class BackgroundServiceHelper
    {
        public static void CreateAndStart<I, T>(IServiceCollection services, params object[] constructorArgs)
        {
            // hack??
            // we want a type created as a hosted service and running but still be able to
            // get a reference to it using the given interface.
            // Is there a better way to do this, then the following two steps?
            services.AddSingleton(typeof(I), x =>
            {
                var s = ActivatorUtilities.CreateInstance(x, typeof(T), constructorArgs);
                return s;
            });
            services.AddSingleton<IHostedService>(x =>
            {
                // here we trigger, the createinstance defined in the previous step
                var s = x.GetService(typeof(I)) as IHostedService;
                return s;
            });
        }
    }
}
