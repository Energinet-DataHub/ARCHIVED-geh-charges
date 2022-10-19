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
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers.Mappers;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.FunctionHost.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.FunctionHost.MarketParticipant
{
    public class MarketParticipantPersisterEndpoint
    {
        private const string FunctionName = nameof(MarketParticipantPersisterEndpoint);
        private readonly ISharedIntegrationEventParser _sharedIntegrationEventParser;
        private readonly IMarketParticipantUpdatedCommandHandler _marketParticipantUpdatedCommandHandler;
        private readonly IMarketParticipantGridAreaUpdatedCommandHandler _marketParticipantGridAreaUpdatedCommandHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public MarketParticipantPersisterEndpoint(
            ISharedIntegrationEventParser sharedIntegrationEventParser,
            IMarketParticipantUpdatedCommandHandler marketParticipantUpdatedCommandHandler,
            IMarketParticipantGridAreaUpdatedCommandHandler marketParticipantGridAreaUpdatedCommandHandler,
            ILoggerFactory loggerFactory,
            IUnitOfWork unitOfWork)
        {
            _sharedIntegrationEventParser = sharedIntegrationEventParser;
            _marketParticipantUpdatedCommandHandler = marketParticipantUpdatedCommandHandler;
            _marketParticipantGridAreaUpdatedCommandHandler = marketParticipantGridAreaUpdatedCommandHandler;
            _unitOfWork = unitOfWork;
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
            var eventType = messageEvent.GetType().Name;

            _logger.LogInformation(
                "Received integration events from Market Participant of type {Type}",
                messageEvent.GetType());

            switch (eventType)
            {
                case nameof(ActorUpdatedIntegrationEvent):
                    {
                        var command = MarketParticipantIntegrationEventMapper.Map(
                            (ActorUpdatedIntegrationEvent)messageEvent);
                        await _marketParticipantUpdatedCommandHandler.HandleAsync(command).ConfigureAwait(false);
                        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
                        break;
                    }

                case nameof(GridAreaUpdatedIntegrationEvent):
                    {
                        var command = MarketParticipantIntegrationEventMapper.Map(
                            (GridAreaUpdatedIntegrationEvent)messageEvent);
                        await _marketParticipantGridAreaUpdatedCommandHandler.HandleAsync(command).ConfigureAwait(false);
                        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
                        break;
                    }
            }
        }
    }
}
