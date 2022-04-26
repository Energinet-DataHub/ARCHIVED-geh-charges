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
using System.Threading.Tasks;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using GreenEnergyHub.Charges.Application.GridAreas.Handlers;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.FunctionHost.Common;
using JetBrains.Annotations;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.MarketParticipant
{
    public class MarketParticipantEndpoint
    {
        public const string FunctionName = nameof(MarketParticipantEndpoint);
        private readonly ISharedIntegrationEventParser _sharedIntegrationEventParser;
        private readonly IMarketParticipantPersister _marketParticipantPersister;
        private readonly IGridAreaPersister _gridAreaPersister;

        public MarketParticipantEndpoint(
            ISharedIntegrationEventParser sharedIntegrationEventParser,
            IMarketParticipantPersister marketParticipantPersister,
            IGridAreaPersister gridAreaPersister)
        {
            _sharedIntegrationEventParser = sharedIntegrationEventParser;
            _marketParticipantPersister = marketParticipantPersister;
            _gridAreaPersister = gridAreaPersister;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.MarketParticipantChangedTopicName + "%",
                "%" + EnvironmentSettingNames.MarketParticipantChangedSubscriptionName + "%",
                Connection = EnvironmentSettingNames.DataHubListenerConnectionString)]
            [NotNull] byte[] message)
        {
             var messageEvent = _sharedIntegrationEventParser.Parse(message);
             ArgumentNullException.ThrowIfNull(messageEvent);
             if (messageEvent.GetType().IsAssignableFrom(typeof(IActorUpdatedIntegrationEventParser)))
             {
                 var marketParticipantChangedEvent =
                     MarketParticipantChangedEventMapper.MapFromActor((ActorUpdatedIntegrationEvent)messageEvent);
                 await _marketParticipantPersister
                     .PersistAsync(marketParticipantChangedEvent)
                     .ConfigureAwait(false);
             }

             if (messageEvent.GetType().IsAssignableFrom(typeof(IGridAreaUpdatedIntegrationEventParser)))
             {
                 // GridArea
                 var gridAreaChangedEvent =
                     MarketParticipantChangedEventMapper.MapFromGridArea((GridAreaUpdatedIntegrationEvent)messageEvent);
                 await _gridAreaPersister
                     .PersistAsync(gridAreaChangedEvent)
                     .ConfigureAwait(false);
             }
        }
    }
}
