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

using System;
using System.Threading.Tasks;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers.Mappers;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.FunctionHost.Common;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.MarketParticipant
{
    public class MarketParticipantGridAreaAddedEndpoint
    {
        private const string FunctionName = nameof(MarketParticipantGridAreaAddedEndpoint);
        private readonly ISharedIntegrationEventParser _sharedIntegrationEventParser;
        private readonly IGridAreaAddedToMarketParticipantCommandHandler _gridAreaAddedToMarketParticipantCommandHandler;
        private readonly IUnitOfWork _unitOfWork;

        public MarketParticipantGridAreaAddedEndpoint(
            ISharedIntegrationEventParser sharedIntegrationEventParser,
            IGridAreaAddedToMarketParticipantCommandHandler gridAreaAddedToMarketParticipantCommandHandler,
            IUnitOfWork unitOfWork)
        {
            _sharedIntegrationEventParser = sharedIntegrationEventParser;
            _gridAreaAddedToMarketParticipantCommandHandler = gridAreaAddedToMarketParticipantCommandHandler;
            _unitOfWork = unitOfWork;
        }

        [Function(FunctionName)]
        public async Task RunAsync([ServiceBusTrigger(
            "%" + EnvironmentSettingNames.IntegrationEventTopicName + "%",
            "%" + EnvironmentSettingNames.GridAreaAddedToActorSubscriptionName + "%",
            Connection = EnvironmentSettingNames.DataHubListenerConnectionString)]
            byte[] message)
        {
            var gridAreaAddedEvent = (GridAreaAddedToActorIntegrationEvent)_sharedIntegrationEventParser.Parse(message);
            var command = MarketParticipantIntegrationEventMapper.Map(gridAreaAddedEvent);
            await _gridAreaAddedToMarketParticipantCommandHandler.HandleAsync(command).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
