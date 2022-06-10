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

using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry.Persistence.Actors;

namespace GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry.MarketParticipantsSynchronization
{
    public static class MarketParticipantUpdater
    {
        public static void Update(
            MarketParticipant marketParticipant,
            Actor actor,
            MarketParticipantRole businessProcessRole)
        {
            UpdateMarketParticipantId(marketParticipant, actor.IdentificationNumber);
            UpdateIsActive(marketParticipant, actor.Active);
            UpdateRole(marketParticipant, businessProcessRole);
        }

        /// <summary>
        /// This is NOT a legal business operation. It is however supported during the implementation of the
        /// temporary market participant registry solution where such updates apparently occurs.
        /// </summary>
        private static void UpdateMarketParticipantId(MarketParticipant marketParticipant, string marketParticipantId)
        {
            if (marketParticipant.MarketParticipantId == marketParticipantId) return;
            SetMarketParticipantId(marketParticipant, marketParticipantId);
        }

        private static void UpdateIsActive(MarketParticipant marketParticipant, bool isActive)
        {
            if (marketParticipant.IsActive == isActive) return;
            marketParticipant.IsActive = isActive;
        }

        private static void UpdateRole(MarketParticipant marketParticipant, MarketParticipantRole businessProcessRole)
        {
            if (marketParticipant.BusinessProcessRole == businessProcessRole) return;
            marketParticipant.UpdateBusinessProcessRole(businessProcessRole);
        }

        private static void SetMarketParticipantId(MarketParticipant marketParticipant, string marketParticipantId)
        {
            var prop = marketParticipant.GetType().GetProperty(nameof(MarketParticipant.MarketParticipantId))!;
            prop.SetValue(marketParticipant, marketParticipantId);
        }
    }
}
