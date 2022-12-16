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
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.Events;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation
{
    public class ChargeHistoryPersister : IChargeHistoryPersister
    {
        private readonly IChargeHistoryFactory _chargeHistoryFactory;
        private readonly IChargeHistoryRepository _chargeHistoryRepository;

        public ChargeHistoryPersister(
            IChargeHistoryFactory chargeHistoryFactory,
            IChargeHistoryRepository chargeHistoryRepository)
        {
            _chargeHistoryFactory = chargeHistoryFactory;
            _chargeHistoryRepository = chargeHistoryRepository;
        }

        public async Task PersistHistoryAsync(ChargeInformationOperationsAcceptedEvent chargeInformationOperationsAcceptedEvent)
        {
            ArgumentNullException.ThrowIfNull(chargeInformationOperationsAcceptedEvent);
            var chargeHistories = _chargeHistoryFactory.Create(chargeInformationOperationsAcceptedEvent);

            await _chargeHistoryRepository.AddRangeAsync(chargeHistories).ConfigureAwait(false);
        }
    }
}
