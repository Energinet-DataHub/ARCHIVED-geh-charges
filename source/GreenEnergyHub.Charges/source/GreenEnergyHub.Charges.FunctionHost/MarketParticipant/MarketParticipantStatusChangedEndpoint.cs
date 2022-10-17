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
    public class MarketParticipantStatusChangedEndpoint
    {
        private const string FunctionName = nameof(MarketParticipantStatusChangedEndpoint);
        private readonly IActorIntegrationEventMapper _actorIntegrationEventMapper;
        private readonly IMarketParticipantStatusChangedCommandHandler _marketParticipantStatusChangedCommandHandler;
        private readonly IUnitOfWork _unitOfWork;

        public MarketParticipantStatusChangedEndpoint(
            IActorIntegrationEventMapper actorIntegrationEventMapper,
            IMarketParticipantStatusChangedCommandHandler marketParticipantStatusChangedCommandHandler,
            IUnitOfWork unitOfWork)
        {
            _actorIntegrationEventMapper = actorIntegrationEventMapper;
            _marketParticipantStatusChangedCommandHandler = marketParticipantStatusChangedCommandHandler;
            _unitOfWork = unitOfWork;
        }

        [Function(FunctionName)]
        public async Task RunAsync([ServiceBusTrigger(
            "%" + EnvironmentSettingNames.IntegrationEventTopicName + "%",
            "%" + EnvironmentSettingNames.MarketParticipantStatusChangedSubscriptionName + "%",
            Connection = EnvironmentSettingNames.DataHubListenerConnectionString)]
            byte[] message)
        {
            var updateStatusCommand = _actorIntegrationEventMapper.MapFromActorStatusChanged(message);
            await _marketParticipantStatusChangedCommandHandler.HandleAsync(updateStatusCommand).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
