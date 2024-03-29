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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Domain.Charges
{
    public class ChargeFactory : IChargeFactory
    {
        private readonly IMarketParticipantRepository _marketParticipantRepository;

        public ChargeFactory(IMarketParticipantRepository marketParticipantRepository)
        {
            _marketParticipantRepository = marketParticipantRepository;
        }

        public async Task<Charge> CreateFromChargeOperationDtoAsync(ChargeInformationOperationDto chargeInformationOperationDto)
        {
            var owner = await _marketParticipantRepository
                .SingleOrNullAsync(chargeInformationOperationDto.ChargeOwner)
                .ConfigureAwait(false);

            if (owner == null)
                throw new InvalidOperationException($"Market participant '{chargeInformationOperationDto.ChargeOwner}' does not exist.");

            return Charge.Create(
                chargeInformationOperationDto.OperationId,
                chargeInformationOperationDto.ChargeName,
                chargeInformationOperationDto.ChargeDescription,
                chargeInformationOperationDto.SenderProvidedChargeId,
                owner.Id,
                chargeInformationOperationDto.ChargeType,
                chargeInformationOperationDto.Resolution,
                chargeInformationOperationDto.TaxIndicator,
                chargeInformationOperationDto.VatClassification,
                chargeInformationOperationDto.TransparentInvoicing == TransparentInvoicing.Transparent,
                chargeInformationOperationDto.StartDateTime,
                chargeInformationOperationDto.EndDateTime);
        }
    }
}
