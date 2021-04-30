using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using System.IO;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using EMS.WebHost.Helpers;

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

            var settings = new JwtSettings()
            {
                Key = "72cc7881-297d-4670-8d95-54a00692f1ab",
                Issuer = "http://petteflet.org",
                Audience = "Test",
                MinutesToExpiration = 5,
                ClockSkew = new System.TimeSpan(0, 0, 30)
            };

            // don't map any claims.. we don't need old style xml schema claims...
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = new RsaSecurityKey(JwtTokens.KeyParamaters),
                    ClockSkew = settings.ClockSkew
                };
                options.SaveToken = true;
                options.Events = new JwtBearerEvents
                {
                    //OnMessageReceived = context =>
                    //{
                    //    // retrieve jwt from cookie and store in context
                    //    if (context.Request.Cookies.ContainsKey("X-Access-Token"))
                    //    {
                    //        context.Token = context.Request.Cookies["X-Access-Token"];
                    //    }

                    //    return Task.CompletedTask;
                    //}
                };
            //})
            //.AddCookie(options =>
            //{
            //    options.Cookie.SameSite = SameSiteMode.Strict;
            //    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            //    options.Cookie.IsEssential = true;
            });

            //In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "dist";
            });
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

            app.UseMiddleware<Middleware.SecurityHeaders>();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.Use(async (context, next) =>
            {
                Logger.LogInformation($"{context.Request.Path}");
                var path = context.Request.Path;
                if (path.StartsWithSegments(new PathString("/app")))
                {
                    if (string.IsNullOrEmpty(Path.GetExtension(path)))
                    {
                        context.Response.ContentType = "text/html";
                        context.Request.Path = Path.Combine("/app", "index.html");
                    }
                }

                if (path.Equals(new PathString("/")))
                {
                    context.Response.ContentType = "text/html";
                    context.Request.Path = Path.Combine("/app", "index.html");
                }
                await next.Invoke();
            });

            app.UseRouting();

            // put this between UseRouting and UseEndpoints
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //app.Use(FallbackMiddlewareHandler);
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
