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
    }
}
