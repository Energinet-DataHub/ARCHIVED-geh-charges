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
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Dtos.MarketParticipantsChangedEvents;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application.MarketParticipants.Handlers
{
    public class MarketParticipantPersister : IMarketParticipantPersister
    {
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public MarketParticipantPersister(
            IMarketParticipantRepository marketParticipantRepository,
            ILoggerFactory loggerFactory,
            IUnitOfWork unitOfWork)
        {
            _marketParticipantRepository = marketParticipantRepository;
            _logger = loggerFactory.CreateLogger(nameof(MarketParticipantPersister));
            _unitOfWork = unitOfWork;
        }

        public async Task PersistAsync(MarketParticipantChangedEvent marketParticipantChangedEvent)
        {
            if (marketParticipantChangedEvent is null)
                throw new ArgumentNullException(nameof(marketParticipantChangedEvent));
            foreach (var businessProcessRole in marketParticipantChangedEvent.BusinessProcessRoles)
            {
                var existingMarketParticipant = await _marketParticipantRepository.GetOrNullAsync(
                    businessProcessRole,
                    marketParticipantChangedEvent.MarketParticipantId).ConfigureAwait(false);

                if (existingMarketParticipant is null)
                {
                    await AddMarketParticipantAsync(
                        marketParticipantChangedEvent,
                        businessProcessRole).ConfigureAwait(false);
                }
                else
                {
                    UpdateMarketParticipant(
                        marketParticipantChangedEvent,
                        existingMarketParticipant,
                        businessProcessRole);
                }
            }

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }

        private void UpdateMarketParticipant(
            MarketParticipantChangedEvent marketParticipantChangedEvent,
            MarketParticipant existingMarketParticipant,
            MarketParticipantRole businessProcessRole)
        {
            existingMarketParticipant.IsActive = marketParticipantChangedEvent.IsActive;
            _logger.LogInformation(
                "Marketparticipant ID '{MarketParticipantId}' with role '{BusinessProcessRole}' has changed state",
                existingMarketParticipant.MarketParticipantId,
                businessProcessRole);
        }

        private async Task AddMarketParticipantAsync(
            MarketParticipantChangedEvent marketParticipantChangedEvent,
            MarketParticipantRole businessProcessRole)
        {
            var marketParticipant = new MarketParticipant(
                Guid.NewGuid(),
                marketParticipantChangedEvent.MarketParticipantId,
                marketParticipantChangedEvent.IsActive,
                businessProcessRole);

            await _marketParticipantRepository.AddAsync(marketParticipant).ConfigureAwait(false);
            var marketParticipantId = marketParticipant.MarketParticipantId;
            _logger.LogInformation(
                "Marketparticipant ID '{MarketParticipantId}' with role '{BusinessProcessRole}' has been persisted",
                marketParticipant.MarketParticipantId,
                businessProcessRole);
        }
    }
}
