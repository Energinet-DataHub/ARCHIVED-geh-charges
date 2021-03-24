using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GreenEnergyHub.Charges.MessageReceiver
{
    public static class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(RegisterServices)
                .Build();

            host.Run();
        }

        private static void RegisterServices(IServiceCollection collection)
        {
            collection.AddScoped<IChangeOfChargesMessageHandler, ChangeOfChargesMessageHandler>();
            collection.AddSingleton<IJsonSerializer, JsonSerializer>();
        }
    }
}
