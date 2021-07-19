using System;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GreenEnergyHub.Charges.ChargeLinkCommandReceiver
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .Build();

            host.Run();
        }

        private static void ConfigureServices(HostBuilderContext hostBuilderContext,
            IServiceCollection serviceCollection)
        {

        }

        private static void ConfigureMessaging(IServiceCollection services)
        {
            services.AddMessagingProtobuf().AddMessageDispatcher<ChargeLinkCommand>(
                GetEnv("CHARGE_LINK_RECEIVED_SENDER_CONNECTION_STRING"),
                GetEnv("CHARGE_LINK_RECEIVED_TOPIC_NAME"));
        }

        private static string GetEnv(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName) ??
                   throw new Exception($"Function app is missing required environment variable '{variableName}'");
        }
    }
}
