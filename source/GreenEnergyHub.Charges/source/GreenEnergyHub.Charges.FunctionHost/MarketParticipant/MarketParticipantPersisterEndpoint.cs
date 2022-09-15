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
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.FunctionHost.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.FunctionHost.MarketParticipant
{
    public class MarketParticipantPersisterEndpoint
    {
        private const string FunctionName = nameof(MarketParticipantPersisterEndpoint);
        private readonly ISharedIntegrationEventParser _sharedIntegrationEventParser;
        private readonly IMarketParticipantEventHandler _marketParticipantEventHandler;
        private readonly ILogger _logger;

        public MarketParticipantPersisterEndpoint(
            ISharedIntegrationEventParser sharedIntegrationEventParser,
            IMarketParticipantEventHandler marketParticipantEventHandler,
            ILoggerFactory loggerFactory)
        {
            _sharedIntegrationEventParser = sharedIntegrationEventParser;
            _marketParticipantEventHandler = marketParticipantEventHandler;
            _logger = loggerFactory.CreateLogger(FunctionName);
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.IntegrationEventTopicName + "%",
                "%" + EnvironmentSettingNames.MarketParticipantChangedSubscriptionName + "%",
                Connection = EnvironmentSettingNames.DataHubListenerConnectionString)]
            byte[] message)
        {
            var messageEvent = _sharedIntegrationEventParser.Parse(message);

            _logger.LogInformation(
                "Received integration events from Market Participant of type {Type}",
                messageEvent.GetType());

            await _marketParticipantEventHandler.HandleAsync(messageEvent).ConfigureAwait(false);
        }
    }
}
