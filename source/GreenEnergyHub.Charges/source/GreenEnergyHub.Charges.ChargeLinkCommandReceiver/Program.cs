using System;
using GreenEnergyHub.Charges.Application.ChargeLinks;
using GreenEnergyHub.Charges.Application.Mapping;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;

namespace GreenEnergyHub.Charges.ChargeLinkCommandReceiver
{
    public static class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(ConfigureServices)
                .Build();

            host.Run();
        }

        private static void ConfigureServices(
            HostBuilderContext hostBuilderContext,
            IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped(typeof(IClock), _ => SystemClock.Instance);
            serviceCollection.AddLogging();
            serviceCollection.AddScoped<IChargeLinkCommandAcceptedHandler, ChargeLinkCommandAcceptedHandler>();

            serviceCollection.AddSingleton<IChargeLinkCommandMapper, ChargeLinkCommandMapper>();

            ConfigureMessaging(serviceCollection);
        }

        private static void ConfigureMessaging(IServiceCollection services)
        {
            services.AddScoped<MessageDispatcher>();
            services.AddMessagingProtobuf().AddMessageDispatcher<ChargeCommandAcceptedEvent>(
                GetEnv("CHARGE_LINK_ACCEPTED_SENDER_CONNECTION_STRING"),
                GetEnv("CHARGE_LINK_ACCEPTED_TOPIC_NAME"));
        }

        private static string GetEnv(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName) ??
                   throw new Exception($"Function app is missing required environment variable '{variableName}'");
        }
    }
}
