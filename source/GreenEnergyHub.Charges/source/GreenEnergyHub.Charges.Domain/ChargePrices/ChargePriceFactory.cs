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
using GreenEnergyHub.Charges.Domain.ChargeInformation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;

namespace GreenEnergyHub.Charges.Domain.ChargePrices
{
    public class ChargePriceFactory : IChargePriceFactory
    {
        private readonly IChargeRepository _chargeInformationRepository;

        public ChargePriceFactory(IChargeRepository chargeInformationRepository)
        {
            _chargeInformationRepository = chargeInformationRepository;
        }

        public async Task<ChargePrice> CreateFromChargeOperationDtoAsync(ChargeOperationDto operation, Point point)
        {
            var chargeIdentifier = new ChargeInformationIdentifier(operation.ChargeId, operation.ChargeOwner, operation.Type);
            var chargeInformation = await _chargeInformationRepository.GetOrNullAsync(chargeIdentifier).ConfigureAwait(false);
            ArgumentNullException.ThrowIfNull(chargeInformation);
            return new ChargePrice(
                Guid.NewGuid(),
                chargeInformation.Id,
                point.Time,
                point.Price);
        }
    }
}
