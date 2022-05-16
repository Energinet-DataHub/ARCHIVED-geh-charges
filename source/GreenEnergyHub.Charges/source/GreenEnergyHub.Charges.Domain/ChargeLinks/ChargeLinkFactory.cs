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
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChargeInformations;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.MeteringPoints;

namespace GreenEnergyHub.Charges.Domain.ChargeLinks
{
    public class ChargeLinkFactory : IChargeLinkFactory
    {
        private readonly IChargeInformationRepository _chargeInformationRepository;
        private readonly IMeteringPointRepository _meteringPointRepository;

        public ChargeLinkFactory(IChargeInformationRepository chargeInformationRepository, IMeteringPointRepository meteringPointRepository)
        {
            _chargeInformationRepository = chargeInformationRepository;
            _meteringPointRepository = meteringPointRepository;
        }

        public async Task<ChargeLink> CreateAsync(ChargeLinkDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var chargeIdentifier = new ChargeIdentifier(dto.SenderProvidedChargeId, dto.ChargeOwner, dto.ChargeType);
            var charge = await _chargeInformationRepository.GetAsync(chargeIdentifier).ConfigureAwait(false);

            var meteringPoint = await _meteringPointRepository
                .GetMeteringPointAsync(dto.MeteringPointId)
                .ConfigureAwait(false);

            var chargeLink = new ChargeLink(
                charge.Id,
                meteringPoint.Id,
                dto.StartDateTime,
                dto.EndDateTime.TimeOrEndDefault(),
                dto.Factor);

            return chargeLink;
        }
    }
}
