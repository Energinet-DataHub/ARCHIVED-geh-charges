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

using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub.Infrastructure;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.ChargeLinkBundle.Cim;
using GreenEnergyHub.Charges.Infrastructure.ChargeLinkBundle.MessageHub;
using GreenEnergyHub.Charges.Infrastructure.Configuration;
using GreenEnergyHub.Charges.Infrastructure.SyncRequest;
using GreenEnergyHub.Json;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    public static class ChargeLinkBundleSenderConfiguration
    {
        public static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ISyncRequestMetaDataFactory, SyncRequestMetaDataFactory>();
            serviceCollection.AddScoped<IJsonSerializer, JsonSerializer>();
            serviceCollection.AddScoped<IChargeLinkBundleSender, ChargeLinkBundleSender>();
            serviceCollection.AddScoped<IChargeLinkBundleCreator, ChargeLinkBundleCreator>();
            serviceCollection.AddScoped<IChargeLinkBundleReplier, ChargeLinkBundleReplier>();
            serviceCollection.AddScoped<IHubSenderConfiguration>(_ =>
            {
                var senderId = EnvironmentHelper.GetEnv(EnvironmentSettingNames.HubSenderId);
                var roleIntText = EnvironmentHelper.GetEnv(EnvironmentSettingNames.HubSenderRoleIntEnumValue);
                return new HubSenderConfiguration(senderId, (MarketParticipantRole)int.Parse(roleIntText));
            });
            serviceCollection.AddScoped<IChargeLinkCimSerializer, ChargeLinkCimSerializer>();
        }
    }
}
