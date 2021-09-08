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

using System;
using GreenEnergyHub.Charges.Application.ChangeOfCharges.Repositories;
using GreenEnergyHub.Charges.Application.ChargeLinks.Factories;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Application.ChargeLinks.Mapping;
using GreenEnergyHub.Charges.Application.Charges.Repositories;
using GreenEnergyHub.Charges.Domain.ChargeLinks.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Messaging.Protobuf;
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
            serviceCollection.AddScoped<IChargeLinkCommandReceivedHandler, ChargeLinkCommandReceivedHandler>();
            serviceCollection.AddScoped<IChargeLinkFactory, ChargeLinkFactory>();
            serviceCollection.AddScoped<IChargeRepository, IChargeRepository>();
            serviceCollection.AddScoped<IMeteringPointRepository, MeteringPointRepository>();
            serviceCollection.AddScoped<IChargesDatabaseContext, ChargesDatabaseContext>();

            serviceCollection.AddSingleton<IChargeLinkCommandMapper, ChargeLinkCommandMapper>();

            ConfigureMessaging(serviceCollection);
        }

        private static void ConfigureMessaging(IServiceCollection services)
        {
            services.ReceiveProtobuf<ChargeLinkCommandReceivedContract>(
                configuration => configuration.WithParser(() => ChargeLinkCommandReceivedContract.Parser));
            services.SendProtobuf<ChargeLinkCommandAcceptedContract>();
            services.AddMessagingProtobuf().AddMessageDispatcher<ChargeLinkCommandAcceptedEvent>(
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
