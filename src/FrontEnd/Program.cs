using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;

namespace FrontEnd
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var hostConfig = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "ASPNETCORE_")
                .AddDockerSecrets()
                .AddCommandLine(args)
                .Build();

            var hostBuilder = WebHost.CreateDefaultBuilder(args);
            var instrumentationKey = hostConfig["ApplicationInsights:InstrumentationKey"];
            if(!string.IsNullOrEmpty(instrumentationKey))
            {
                Console.WriteLine("Using Application Insights");
                hostBuilder.UseApplicationInsights(instrumentationKey);
            }

            return hostBuilder.UseUrls("http://localhost:55994")
                .UseConfiguration(hostConfig)
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddDockerSecrets();
                })
                .UseStartup<Startup>()
                .Build();
        }
    }
}
