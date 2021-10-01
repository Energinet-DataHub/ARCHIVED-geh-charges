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

using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeRejection;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandRejected;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Messaging.Protobuf;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeRejectionSenderConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IChargeRejectionSender, ChargeRejectionSender>();

            serviceCollection.ReceiveProtobufMessage<ChargeCommandRejectedContract>(
                configuration => configuration.WithParser(() => ChargeCommandRejectedContract.Parser));
            serviceCollection.SendProtobuf<ChargeRejectionContract>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeRejection>(
                EnvironmentHelper.GetEnv("DOMAINEVENT_SENDER_CONNECTION_STRING"),
                EnvironmentHelper.GetEnv("POST_OFFICE_TOPIC_NAME"));
        }
    }
}
