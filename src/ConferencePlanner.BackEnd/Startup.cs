using System.Threading.Tasks;
using ConferencePlanner.BackEnd.Data;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;

namespace ConferencePlanner.BackEnd
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configuration Required from the environment:
            // * Authentication:Tenant
            // * Authentication:ClientId
            // * ConnectionStrings:DefaultConnectionString

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = $"https://login.microsoftonline.com/tfp/{Configuration["Authentication:Tenant"]}/{Configuration["Authentication:PolicyId"]}/v2.0/";
                    options.Audience = Configuration["Authentication:ClientId"];
                });

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireClaim("http://schemas.microsoft.com/identity/claims/objectidentifier")
                    .Build();
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info { Title = "Conference Planner API", Version = "v1" });
                options.CustomSchemaIds(t => t.FullName);
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Copied from https://github.com/aspnet/AzureIntegration/blob/dev/src/Microsoft.AspNetCore.ApplicationInsights.HostingStartup/ApplicationInsightsLoggerStartupFilter.cs
            var loggerEnabled = true;
            loggerFactory.AddApplicationInsights(app.ApplicationServices, (s, level) => loggerEnabled, () => loggerEnabled = false);

            var telemetryConfiguration = app.ApplicationServices.GetService<TelemetryConfiguration>();
            if (telemetryConfiguration != null)
            {
                telemetryConfiguration.TelemetryInitializers.Add(new ServiceNameTelemetryInitializer("Backend"));
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(options =>
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Conference Planner API v1")
            );

            app.UseAuthentication();

            app.UseMvc();

            app.Run(context =>
            {
                context.Response.Redirect("/swagger");
                return Task.CompletedTask;
            });
        }
    }
}
