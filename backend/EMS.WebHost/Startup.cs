using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.IO.Compression;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using EMS.WebHost.Controllers;
using EMS.WebHost.Helpers;

namespace EMS.WebHost
{
    public record WebConfig
    {
        public string? ContentRootPath { get; set; }
        public string? WebRootPath { get; set; }
        public JwtConfig? Jwt { get; set; }
    }
    public record KestrelConfig
    {
        public EndPoints? EndPoints { get; set; }
    }
    public record EndPoints
    {
        public Http? Http { get; set; }
    }
    [SuppressMessage("", "CA1056")]
    [SuppressMessage("", "CA1724")]
    public record Http
    {
        public string? Url { get; set; }
    }

    public interface IWebHost {
        DateTime UpSince { get; }
    }

    public class Startup : IWebHost
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        private IWebHostEnvironment Env { get; init; }
        private IConfiguration Configuration { get; init; }

        public WebConfig? WebConfig { get; set; }

        private readonly DateTime _upSince = DateTime.Now;
        public DateTime UpSince => _upSince;

        [SuppressMessage("Code Analysis", "CA1810")]
        static Startup()
        {
            // don't map any claims.. we don't need old style xml schema claims...
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
        }

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Env = env;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            WebConfig wc = new();
            Configuration.GetSection("web").Bind(wc);
            WebConfig = wc;

            services.AddSingleton(typeof(IWebHost), this);
            services.AddSingleton<IJwtService, JwtTokenService>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer();

            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                JwtConfig j = new();
                Configuration.GetSection("web:jwt").Bind(j);
                options.TokenValidationParameters = JwtTokenService.CreateTokenValidationParameters(Configuration);
                options.SaveToken = true;
            });

            services.AddControllers().AddApplicationPart(typeof(UsersController).Assembly);

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "HEMS API", Version = "v1" });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                options.OperationFilter<SecurityRequirementsOperationFilter>();
            });

            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.SmallestSize;
            });

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = new[] {
                        "application/font-woff2",
                        "application/javascript",
                        "application/json",
                        "application/octet-stream",
                        "application/wasm",
                        "application/xml",
                        "text/css",
                        "text/javascript",
                        "text/json",
                        "text/html",
                        "text/plain",
                };

                options.EnableForHttps = true;
            });

#if DEBUG
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>{                          
                          builder
                            .AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            ;
                      });
            });

            // insert the AnalysisStartupFilter as the first IStartupFilter in the container
            services.Insert(0, ServiceDescriptor.Transient<IStartupFilter, Microsoft.AspNetCore.MiddlewareAnalysis.AnalysisStartupFilter>());
#endif
        }

        public void Configure(WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

#if DEBUG
            // Grab the "Microsoft.AspNetCore" DiagnosticListener from DI
            var listener = app.Services.GetRequiredService<DiagnosticListener>();

            // Create an instance of the AnalysisDiagnosticAdapter using the IServiceProvider
            var observer = ActivatorUtilities.CreateInstance<AnalysisDiagnosticAdapter>(app.Services);

            // Subscribe to the listener with the SubscribeWithAdapter() extension method
            // and ignoring the disposible subscription 
            _ = listener.SubscribeWithAdapter(observer);
#endif

            app.UseResponseCompression();

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

                        await context.Response.WriteAsync("<html lang=\"en\"><body>\r\n").ConfigureAwait(false);
                        await context.Response.WriteAsync("ERROR!<br><br>\r\n").ConfigureAwait(false);

                        var exceptionHandlerPathFeature =
                            context.Features.Get<IExceptionHandlerPathFeature>();

                        if (exceptionHandlerPathFeature?.Error is FileNotFoundException)
                        {
                            await context.Response.WriteAsync("File error thrown!<br><br>\r\n").ConfigureAwait(false);
                        }

                        await context.Response.WriteAsync("<a href=\"/\">Home</a><br>\r\n").ConfigureAwait(false);
                        await context.Response.WriteAsync("</body></html>\r\n").ConfigureAwait(false);
                        await context.Response.WriteAsync(new string(' ', 512)).ConfigureAwait(false);
                    });
                });
                app.UseHsts();
            }

            if (!Env.IsDevelopment())
                app.UseHttpsRedirection();
            
            app.UseMiddleware<Middleware.SecurityHeaders>();
            app.UseDefaultFiles();

            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings.Clear();
            // Add new mappings
            provider.Mappings[".blat"] = "application/octet-stream";
            provider.Mappings[".br"] = "application/x-brotli";
            provider.Mappings[".css"] = "text/css";
            provider.Mappings[".dll"] = "application/octet-stream";
            provider.Mappings[".gif"] = "image/gif";
            provider.Mappings[".htm"] = "text/html";
            provider.Mappings[".html"] = "text/html";
            provider.Mappings[".dat"] = "application/octet-stream";
            provider.Mappings[".jpg"] = "image/jpg";
            provider.Mappings[".js"] = "text/javascript";
            provider.Mappings[".json"] = "application/json";
            provider.Mappings[".png"] = "image/png";
            provider.Mappings[".wasm"] = "application/wasm";
            provider.Mappings[".woff"] = "application/font-woff";
            provider.Mappings[".woff2"] = "application/font-woff";

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Env.WebRootPath),
                RequestPath = string.Empty,
                ContentTypeProvider = provider,
                DefaultContentType = "application/octet-stream",
                ServeUnknownFileTypes = true,
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append(
                         "Cache-Control", $"public, max-age={1}");
                }
            });

            app.UseRouting();

            // put this between UseRouting and map of controllers / swagger
            // and CORS needs to be before response caching
            app.UseCors(MyAllowSpecificOrigins);
            app.UseResponseCaching();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapSwagger();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "HEMS API V1");
            });

            app.MapFallbackToFile("index.html");

            app.Use((context, next) =>
            {
                Logger.Info("{Path}", context.Request.Path);
                return next.Invoke();
            });

            Logger.Info("============================================================");
            Logger.Info("Web Host Environment");
            Logger.Info("ApplicationName: {ApplicationName}", Env.ApplicationName);
            Logger.Info("EnvironmentName: {EnvironmentName}", Env.EnvironmentName);
            Logger.Info("ContentRootPath: {ContentRootPath}", Env.ContentRootPath);
            Logger.Info("WebRootPath {WebRootPath}", Env.WebRootPath);
        }
    }
}
