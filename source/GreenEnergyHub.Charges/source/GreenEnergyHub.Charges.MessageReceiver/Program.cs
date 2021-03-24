// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
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
            collection.AddScoped<IJsonSerializer, JsonSerializer>();
            collection.AddScoped<IChangeOfChargesTransactionHandler, ChangeOfChargesTransactionHandler>();
            collection.AddScoped<ILocalEventPublisher, LocalEventPublisher>();
        }
    }
}
