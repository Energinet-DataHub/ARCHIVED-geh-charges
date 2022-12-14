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
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers.Mappers;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.FunctionHost.Common;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.MarketParticipant
{
    public class MarketParticipantB2CActorIdChangedEndpoint
    {
        private const string FunctionName = nameof(MarketParticipantB2CActorIdChangedEndpoint);
        private readonly ISharedIntegrationEventParser _sharedIntegrationEventParser;
        private readonly IMarketParticipantB2CActorIdChangedCommandHandler _marketParticipantB2CActorIdChangedCommandHandler;
        private readonly IUnitOfWork _unitOfWork;

        public MarketParticipantB2CActorIdChangedEndpoint(
            ISharedIntegrationEventParser sharedIntegrationEventParser,
            IMarketParticipantB2CActorIdChangedCommandHandler marketParticipantB2CActorIdChangedCommandHandler,
            IUnitOfWork unitOfWork)
        {
            _sharedIntegrationEventParser = sharedIntegrationEventParser;
            _marketParticipantB2CActorIdChangedCommandHandler = marketParticipantB2CActorIdChangedCommandHandler;
            _unitOfWork = unitOfWork;
        }

        [Function(FunctionName)]
        public async Task RunAsync([ServiceBusTrigger(
            "%" + EnvironmentSettingNames.IntegrationEventTopicName + "%",
            "%" + EnvironmentSettingNames.MarketParticipantB2CActorIdChangedSubscriptionName + "%",
            Connection = EnvironmentSettingNames.DataHubListenerConnectionString)]
            byte[] message)
        {
            var externalIdChangedEvent = (ActorExternalIdChangedIntegrationEvent)_sharedIntegrationEventParser.Parse(message);
            var command = MarketParticipantIntegrationEventMapper.Map(externalIdChangedEvent);
            await _marketParticipantB2CActorIdChangedCommandHandler.HandleAsync(command).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
