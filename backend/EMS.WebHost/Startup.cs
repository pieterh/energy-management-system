using System.IdentityModel.Tokens.Jwt;
using System.IO;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using EMS.WebHost.Helpers;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

namespace EMS.WebHost
{
    public record WebConfig
    {
        public ushort Port { get; set; }
        public JwtConfig? Jwt { get; set; }
        public bool https { get; set; }
    }

    public class Startup
    {
        private const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public WebConfig? WebConfig { get; set; }
        private ILogger? Logger { get; set; }

        public IWebHostEnvironment Env => _env;
        public IConfiguration Configuration => _configuration;

        [SuppressMessage("Code Analysis", "CA1810")]
        static Startup()
        {
            // don't map any claims.. we don't need old style xml schema claims...
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
        }

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            WebConfig wc = new();
            Configuration.GetSection("web").Bind(wc);
            WebConfig = wc;

            services.AddSingleton<IJwtService, JwtTokenService>();

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HEMS API", Version = "v1" });
            });

            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                JwtConfig j = new();
                Configuration.GetSection("web:jwt").Bind(j);
                options.TokenValidationParameters = JwtTokenService.CreateTokenValidationParameters(Configuration);
                options.SaveToken = true;
            });

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();

                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] {
                        "image/jpeg",
                        "image/png",
                        "application/font-woff2",
                        "image/svg+xml",
                        "application/octet-stream",
                        "application/wasm"
                });

                options.EnableForHttps = true;
            });

            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.SmallestSize;
            });

#if DEBUG
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          builder.WithOrigins("http://127.0.0.1:5005",
                                              "http://localhost:5005",
                                              "http://127.0.0.1:5281",
                                              "http://localhost:5281"
                                              )
                          .AllowCredentials()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
            });
#endif
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
            app.UseResponseCompression();

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
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.Combine(Env.ContentRootPath, "dist")),
                RequestPath = string.Empty,
                ContentTypeProvider = provider,
                DefaultContentType = "application/octet-stream",
                ServeUnknownFileTypes = true
            });

            app.UseRouting();

            app.UseCors(MyAllowSpecificOrigins);

            // put this between UseRouting and UseEndpoints
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapSwagger();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "HEMS API V1");
            });
            app.Use((context, next) =>
            {
                Logger.LogInformation("{Path}", context.Request.Path);
                return next.Invoke();
            });

            Logger.LogInformation("============================================================");
            Logger.LogInformation("Web Host Environment");
            Logger.LogInformation("ApplicationName: {ApplicationName}", _env.ApplicationName);
            Logger.LogInformation("EnvironmentName: {EnvironmentName}", _env.EnvironmentName);
            Logger.LogInformation("ContentRootPath: {ContentRootPath}", _env.ContentRootPath);
            Logger.LogInformation("WebRootPath {WebRootPath}", _env.WebRootPath);
        }
    }
}
