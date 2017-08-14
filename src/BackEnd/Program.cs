using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace BackEnd
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
                .AddDockerSecrets("/run/secrets", optional: true)
                .AddCommandLine(args)
                .Build();

            var hostBuilder = WebHost.CreateDefaultBuilder(args);
            var instrumentationKey = hostConfig["ApplicationInsights:InstrumentationKey"];
            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                Console.WriteLine("Using Application Insights");
                hostBuilder.UseApplicationInsights(instrumentationKey.Trim());
            }

            return hostBuilder.UseUrls("http://localhost:56009")
                .UseConfiguration(hostConfig)
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddDockerSecrets("/run/secrets", optional: true);
                })
                .UseStartup<Startup>()
                .Build();
        }
    }
}
