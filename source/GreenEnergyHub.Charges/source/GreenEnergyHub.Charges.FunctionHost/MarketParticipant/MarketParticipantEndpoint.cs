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

using System.Threading.Tasks;
using Energinet.DataHub.MarketParticipant.Integration.Model.Exceptions;
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
        private readonly IActorUpdatedIntegrationEventParser _actorUpdatedIntegrationEventParser;
        private readonly IGridAreaUpdatedIntegrationEventParser _gridAreaUpdatedIntegrationEventParser;
        private readonly IMarketParticipantPersister _marketParticipantPersister;
        private readonly IGridAreaPersister _gridAreaPersister;

        public MarketParticipantEndpoint(
            IActorUpdatedIntegrationEventParser actorUpdatedIntegrationEventParser,
            IGridAreaUpdatedIntegrationEventParser gridAreaUpdatedIntegrationEventParser,
            IMarketParticipantPersister marketParticipantPersister,
            IGridAreaPersister gridAreaPersister)
        {
            _actorUpdatedIntegrationEventParser = actorUpdatedIntegrationEventParser;
            _gridAreaUpdatedIntegrationEventParser = gridAreaUpdatedIntegrationEventParser;
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
            // We don't know which event is thrown, nothing in the message indicates which type of parser to use.
            // Titans is looking into that problem, for now we try to parse the message with all parsers that apply
            // to this domain until we succeed or all has failed.
             try
             {
                // MarketParticipant
                var actorUpdatedIntegrationEvent = _actorUpdatedIntegrationEventParser.Parse(message);
                var marketParticipantChangedEvent =
                    MarketParticipantChangedEventMapper.MapFromActor(actorUpdatedIntegrationEvent);
                await _marketParticipantPersister
                    .PersistAsync(marketParticipantChangedEvent)
                    .ConfigureAwait(false);
             }
             catch (MarketParticipantException)
             {
                 // GridArea
                 var gridAreaUpdatedIntegrationEvent = _gridAreaUpdatedIntegrationEventParser.Parse(message);
                 var gridAreaChangedEvent =
                     MarketParticipantChangedEventMapper.MapFromGridArea(gridAreaUpdatedIntegrationEvent);
                 await _gridAreaPersister
                     .PersistAsync(gridAreaChangedEvent)
                     .ConfigureAwait(false);
             }
        }
    }
}
