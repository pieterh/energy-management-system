using System;
using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EMS.WebHost
{
    public class WebConfig
    {
        public ushort Port { get; set; }
    }

    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public WebConfig WebConfig { get; set; }
        private ILogger Logger { get; set; }

        public IWebHostEnvironment Env => _env;
        public IConfiguration Configuration => _configuration;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            WebConfig wc = new ();
            Configuration.GetSection("web").Bind(wc);
            WebConfig = wc;

            services.AddControllers();

            //In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "dist";
            });

            //services
            //  .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)              
            //  .AddCookie(options =>
            //  {
            //      options.Cookie.HttpOnly = true;
            //  });
        }

        public void Configure(ILogger<Startup> logger, IApplicationBuilder app)
        {
            Logger = logger;

            if (Env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "text/html";

                        await context.Response.WriteAsync("<html lang=\"en\"><body>\r\n");
                        await context.Response.WriteAsync("ERROR!<br><br>\r\n");

                        var exceptionHandlerPathFeature =
                            context.Features.Get<IExceptionHandlerPathFeature>();

                        if (exceptionHandlerPathFeature?.Error is FileNotFoundException)
                        {
                            await context.Response.WriteAsync(
                                                      "File error thrown!<br><br>\r\n");
                        }

                        await context.Response.WriteAsync(
                                                      "<a href=\"/\">Home</a><br>\r\n");
                        await context.Response.WriteAsync("</body></html>\r\n");
                        await context.Response.WriteAsync(new string(' ', 512));
                    });
                });
                app.UseHsts();
            }

            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();

            if (!Env.IsDevelopment())
                app.UseHttpsRedirection();

            app.UseDefaultFiles();

            app.UseStaticFiles();
            //app.UseSpaStaticFiles();

            app.UseRouting();

            // put this between UseRouting and UseEndpoints
            //app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSpa(spa =>
            {
                if (Env.IsDevelopment())
                {
                    // Make sure you have started the frontend with npm run dev on port 5010
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:5010");
                }
            });

            app.Use((context, next) => {
                Logger.LogInformation($"{context.Request.Path}");
                return next.Invoke();
            });
        }

    }
}
