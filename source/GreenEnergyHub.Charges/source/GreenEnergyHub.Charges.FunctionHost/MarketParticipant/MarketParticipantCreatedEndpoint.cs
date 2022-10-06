﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.FunctionHost.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.FunctionHost.MarketParticipant
{
    public class MarketParticipantCreatedEndpoint
    {
        private const string FunctionName = nameof(MarketParticipantCreatedEndpoint);
        private readonly ISharedIntegrationEventParser _sharedIntegrationEventParser;
        private readonly IMarketParticipantEventHandler _marketParticipantEventHandler;
        private readonly ILogger _logger;

        public MarketParticipantCreatedEndpoint(
            ILoggerFactory loggerFactory,
            ISharedIntegrationEventParser sharedIntegrationEventParser,
            IMarketParticipantEventHandler marketParticipantEventHandler)
        {
            _sharedIntegrationEventParser = sharedIntegrationEventParser;
            _marketParticipantEventHandler = marketParticipantEventHandler;
            _logger = loggerFactory.CreateLogger(FunctionName);
        }

        [Function("MarketParticipantCreatedEndpoint")]
        public async Task RunAsync([ServiceBusTrigger(
            "%" + EnvironmentSettingNames.IntegrationEventTopicName + "%",
            "%" + EnvironmentSettingNames.MarketParticipantCreatedSubscriptionName + "%",
            Connection = EnvironmentSettingNames.DataHubListenerConnectionString)]
            byte[] message)
        {
            var messageEvent = (ActorCreatedIntegrationEvent)_sharedIntegrationEventParser.Parse(message);

            _logger.LogInformation("Received Market Participant Created integration event");

            await _marketParticipantEventHandler.HandleMarketParticipantCreatedIntegrationEventAsync(messageEvent)
                .ConfigureAwait(false);
        }
    }
}
