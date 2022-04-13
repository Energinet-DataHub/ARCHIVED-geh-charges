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
using Energinet.DataHub.MarketParticipant.Integration.Model.Exceptions;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.MarketParticipantsChangedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using JetBrains.Annotations;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.FunctionHost.MarketParticipant
{
    public class MarketParticipantEndpoint
    {
        public const string FunctionName = nameof(MarketParticipantEndpoint);
        private readonly IActorUpdatedIntegrationEventParser _actorUpdatedIntegrationEventParser;
        private readonly IGridAreaUpdatedIntegrationEventParser _gridAreaUpdatedIntegrationEventParser;
        private readonly IMarketParticipantPersister _marketParticipantPersister;

        public MarketParticipantEndpoint(
            IActorUpdatedIntegrationEventParser actorUpdatedIntegrationEventParser,
            IGridAreaUpdatedIntegrationEventParser gridAreaUpdatedIntegrationEventParser,
            IMarketParticipantPersister marketParticipantPersister)
        {
            _actorUpdatedIntegrationEventParser = actorUpdatedIntegrationEventParser;
            _gridAreaUpdatedIntegrationEventParser = gridAreaUpdatedIntegrationEventParser;
            _marketParticipantPersister = marketParticipantPersister;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.MarketParticipantChangedTopicName + "%",
                "%" + EnvironmentSettingNames.MarketParticipantChangedSubscriptionName + "%",
                Connection = EnvironmentSettingNames.DataHubListenerConnectionString)]
            [NotNull] byte[] message)
        {
            // Todo: Refactor this when Titans have found a solution
            // We don't now which event is thrown, nothing in the message indicates which type of parser to use.
            // Titans is looking into that problem, for now we try to parse the message with all parsers that apply
            // to this domain until we succeed or all has failed.
            MarketParticipantChangedEvent marketParticipantChangedEvent;
            try
            {
                var actorUpdatedIntegrationEvent = _actorUpdatedIntegrationEventParser.Parse(message);
                marketParticipantChangedEvent =
                    MarketParticipantChangedEventMapper.MapFromActor(actorUpdatedIntegrationEvent);
            }
            catch (MarketParticipantException)
            {
                var gridUpdatedIntegrationEvent = _gridAreaUpdatedIntegrationEventParser.Parse(message);
                marketParticipantChangedEvent =
                    MarketParticipantChangedEventMapper.MapFromGridArea(gridUpdatedIntegrationEvent);
            }

            await _marketParticipantPersister
                .PersistAsync(marketParticipantChangedEvent)
                .ConfigureAwait(false);
        }
    }
}
