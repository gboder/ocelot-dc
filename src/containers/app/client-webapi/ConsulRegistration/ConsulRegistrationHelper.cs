using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ais.Common.ServiceRegistration
{
    public static class ConsulRegistrationHelper
    {
        private static ILogger Logger { get; set; }
        public static ConsulRegistrationOptions Configuration { get; private set; }

        public static IServiceCollection AddConsulConfig(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            Configuration = new ConsulRegistrationOptions();
            configuration.GetSection(nameof(ConsulRegistrationOptions)).Bind(Configuration);

            if (string.IsNullOrWhiteSpace(Configuration.RegistrationToken))
            {
                var logger = services.BuildServiceProvider().GetService<ILogger>();
                logger.LogError($"{nameof(Configuration.RegistrationToken)} is not set.");
            }

            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                consulConfig.Address = new Uri(Configuration.ConsulAddress);
                consulConfig.Token = Configuration.RegistrationToken;
            }));

            return services;
        }

        public static IApplicationBuilder UseConsul(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            Logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(ConsulRegistrationHelper));

            try
            {

                IConsulClient consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
                IHostApplicationLifetime hostApplicationLifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

                Register(Configuration, consulClient, hostApplicationLifetime);
            }
            catch (Exception e)
            {
                Logger.LogCritical(9, e, "Unable to register in Consul.");
            }
            return app;
        }

        internal static void Register(ConsulRegistrationOptions configuration, IConsulClient consulClient, IHostApplicationLifetime hostApplicationLifetime)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (consulClient is null)
            {
                throw new ArgumentNullException(nameof(consulClient));
            }

            var serviceRegistration = BuildServiceRegistration(Configuration);

            RegisterAgentAsync(consulClient, serviceRegistration, hostApplicationLifetime).GetAwaiter().GetResult();
        }

        private static async Task RegisterAgentAsync(IConsulClient consulClient,
            AgentServiceRegistration serviceRegistration,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            Logger.LogInformation($"Registering Service ID={serviceRegistration.ID}, Name={serviceRegistration.Name} with URL {serviceRegistration.Address}:{serviceRegistration.Port}");

            // retry register services in consul until succeed if not on local machine
            if (Configuration.RegisterInConsul)
            {
                await ServiceRegister(consulClient, serviceRegistration).ConfigureAwait(false);
            }

            hostApplicationLifetime.ApplicationStopping.Register(() =>
            {
                Logger.LogInformation($"Unregistering Service after application stopping Service ID={serviceRegistration.ID}, Name={serviceRegistration.Name} with URL {serviceRegistration.Address}:{serviceRegistration.Port}");
                consulClient.Agent.ServiceDeregister(serviceRegistration.ID).Wait();
            });
        }

        private static async Task ServiceRegister(IConsulClient consulClient, AgentServiceRegistration serviceRegistration)
        {
            await consulClient.Agent.ServiceRegister(serviceRegistration).ConfigureAwait(false);
        }

        private static AgentServiceRegistration BuildServiceRegistration(ConsulRegistrationOptions config)
        {
            var hostname = GetFQDN();
            var agentUri = BuildServiceAgentUri(hostname);


            var agent = new AgentServiceRegistration
            {
                ID = $"{config.ServiceName}@{agentUri.Host}:{agentUri.Port}",
                Name = config.ServiceName,
                Address = agentUri.Host,
                Port = agentUri.Port,
            };

            agent.Checks = new AgentServiceCheck[]
            {
                new AgentServiceCheck
{
                 HTTP = "http://localhost:80/health",
                 Method = "GET",
                 Name = "/health",
                 Notes = string.Empty,
                 Header = null,
                 Timeout = TimeSpan.FromSeconds(1),
                 Interval = TimeSpan.FromSeconds(10),
                 DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(config.DeregisterRegisterDelayInSec),
             }
            };

            // if (config.HealthChecks != null && config.HealthChecks.Any())
            // {
            // 	agent.Checks = config.HealthChecks.ToAgentServiceChecks(agentUri.Host);
            // }
            // else
            // {
            // 	Logger.LogWarning($"No health checks found for service: {config.ServiceName} in configuration from {config.ServiceAddress}.");
            // }

            return agent;
        }

        // private static AgentServiceCheck[] ToAgentServiceChecks(this List<HealthCheckConfiguration> checkConfigurations, string hostname)
        // 	=> checkConfigurations?.Select(c => ToAgentServiceCheck(c, hostname)).ToArray();

        // private static AgentServiceCheck ToAgentServiceCheck(
        // 	this HealthCheckConfiguration checkConfiguration,
        // 	string hostname)
        // {
        // 	var httpUrl = BuildHealthCheckRequest(checkConfiguration, hostname);
        // 	var agentCheck = new AgentServiceCheck
        // 	{
        // 		HTTP = httpUrl,
        // 		Method = checkConfiguration.Method,
        // 		Name = string.IsNullOrWhiteSpace(checkConfiguration.Name) ? $"{checkConfiguration.Address}.{checkConfiguration.Method}" : checkConfiguration.Name,
        // 		Notes = string.IsNullOrWhiteSpace(checkConfiguration.Notes) ? httpUrl : checkConfiguration.Notes,
        // 		Header = checkConfiguration.Headers,
        // 		Timeout = TimeSpan.FromSeconds(checkConfiguration.Timeout),
        // 		Interval = TimeSpan.FromSeconds(checkConfiguration.Interval),
        // 		DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(checkConfiguration.DeregisterAfter)
        // 	};

        // 	return agentCheck;
        // }

        private static string BuildHealthCheckRequest(string hostname)
        {
            return $"http://{hostname}/health";
        }

        private static Uri BuildServiceAgentUri(string hostname)
        {
            // http://{dockerhostname}:{port ?? 80}
            return new Uri($"http://{hostname}/");
        }

        private static string GetFQDN() => Dns.GetHostName();
        //{
        //    string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
        //    string hostName = Dns.GetHostName();

        //    domainName = "." + domainName;

        //    if (!hostName.EndsWith(domainName, StringComparison.OrdinalIgnoreCase))
        //    {
        //        hostName += domainName;
        //    }

        //    return hostName;
        //}
    }
}