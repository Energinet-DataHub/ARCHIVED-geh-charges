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

using GreenEnergyHub.Charges.Application.ChargeLinks.PostOffice;
using GreenEnergyHub.Charges.Domain.ChargeLinkTransmissionRequest;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeLinkCreatedDataAvailableNotifierConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IChargeLinkCreatedDataAvailableNotifier, ChargeLinkCreatedDataAvailableNotifier>();
            serviceCollection.AddScoped<IChargeLinkTransmissionRequestFactory, ChargeLinkTransmissionRequestFactory>();
            serviceCollection
                .AddScoped<IChargeLinkTransmissionRequestRepository, ChargeLinkTransmissionRequestRepository>();

            // This would be redundant as it is already registered for ChargeLinkEventPublisherConfigurationEndpoint
            //serviceCollection.ReceiveProtobufMessage<ChargeLinkCommandAcceptedContract>(
            //    configuration => configuration.WithParser(() => ChargeLinkCommandAcceptedContract.Parser));
        }
    }
}
