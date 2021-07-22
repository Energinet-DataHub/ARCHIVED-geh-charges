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
using GreenEnergyHub.Charges.Application.ChargeLinks;
using GreenEnergyHub.Charges.Application.Factories;
using GreenEnergyHub.Charges.Domain.Events.Integration;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeLinkCreated;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GreenEnergyHub.Charges.ChargeLinkEventPublisher
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

        private static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging();

            ConfigureMessaging(serviceCollection);

            serviceCollection.AddScoped<IChargeLinkCreatedEventFactory, ChargeLinkCreatedEventFactory>();
            serviceCollection.AddScoped<IChargeLinkEventPublishHandler, ChargeLinkEventPublishHandler>();
        }

        private static void ConfigureMessaging(IServiceCollection serviceCollection)
        {
            serviceCollection.ReceiveProtobuf<ChargeLinkCommandAcceptedContract>(
                configuration => configuration.WithParser(() => ChargeLinkCommandAcceptedContract.Parser));
            serviceCollection.SendProtobuf<ChargeLinkCreatedContract>();

            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeLinkCreatedEvent>(
                GetEnv("INTEGRATIONEVENT_SENDER_CONNECTION_STRING"),
                GetEnv("CHARGE_LINK_RECEIVED_TOPIC_NAME"));
        }

        private static string GetEnv(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName) ??
                   throw new Exception($"Function app is missing required environment variable '{variableName}'");
        }
    }
}
