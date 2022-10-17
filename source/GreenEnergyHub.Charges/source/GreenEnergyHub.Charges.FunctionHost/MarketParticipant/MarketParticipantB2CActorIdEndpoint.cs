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
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.FunctionHost.Common;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.MarketParticipant
{
    public class MarketParticipantB2CActorIdEndpoint
    {
        private const string FunctionName = nameof(MarketParticipantB2CActorIdEndpoint);
        private readonly IActorIntegrationEventMapper _actorIntegrationEventMapper;
        private readonly IMarketParticipantB2CActorIdChangedCommandHandler _marketParticipantB2CActorIdChangedCommandHandler;
        private readonly IUnitOfWork _unitOfWork;

        public MarketParticipantB2CActorIdEndpoint(
            IActorIntegrationEventMapper actorIntegrationEventMapper,
            IMarketParticipantB2CActorIdChangedCommandHandler marketParticipantB2CActorIdChangedCommandHandler,
            IUnitOfWork unitOfWork)
        {
            _actorIntegrationEventMapper = actorIntegrationEventMapper;
            _marketParticipantB2CActorIdChangedCommandHandler = marketParticipantB2CActorIdChangedCommandHandler;
            _unitOfWork = unitOfWork;
        }

        [Function(FunctionName)]
        public async Task RunAsync([ServiceBusTrigger(
            "%" + EnvironmentSettingNames.IntegrationEventTopicName + "%",
            "%" + EnvironmentSettingNames.MarketParticipantExternalActorIdChangedSubscriptionName + "%",
            Connection = EnvironmentSettingNames.DataHubListenerConnectionString)]
            byte[] message)
        {
            var externalIdChanged = _actorIntegrationEventMapper.MapFromActorExternalIdChanged(message);
            await _marketParticipantB2CActorIdChangedCommandHandler.HandleAsync(externalIdChanged).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
