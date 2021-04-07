using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.MessageReceiver;
using GreenEnergyHub.Json;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GreenEnergyHub.Charges.MessageReceiver
{
    public class Startup : FunctionsStartup
    {
        public override void Configure([NotNull] IFunctionsHostBuilder builder)
        {
            // builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IJsonSerializer, JsonSerializer>();
            builder.Services.AddSingleton<IChangeOfChargesMessageHandler, ChangeOfChargesMessageHandler>();
            builder.Services.AddSingleton<IChangeOfChargesTransactionHandler, ChangeOfChargesTransactionHandler>();
            builder.Services.AddSingleton<ILocalEventPublisher, LocalEventPublisher>();
        }
    }
}
