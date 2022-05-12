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

using GreenEnergyHub.Charges.Application.ChargeInformations.Acknowledgement;
using GreenEnergyHub.Charges.Core.Currency;
using GreenEnergyHub.Charges.Domain.ChargeInformations;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;

namespace GreenEnergyHub.Charges.Application.ChargeInformations.Factories
{
    public class ChargeInformationCreatedEventFactory : IChargeInformationCreatedEventFactory
    {
        private readonly CurrencyConfigurationIso4217 _currencyConfigurationIso4217;

        public ChargeInformationCreatedEventFactory(CurrencyConfigurationIso4217 currencyConfigurationIso4217)
        {
            _currencyConfigurationIso4217 = currencyConfigurationIso4217;
        }

        public ChargeInformationCreatedEvent Create(ChargeOperationDto chargeOperationDto)
        {
            return new ChargeInformationCreatedEvent(
                chargeOperationDto.ChargeInformationId,
                chargeOperationDto.Type,
                chargeOperationDto.ChargeOwner,
                _currencyConfigurationIso4217.Currency,
                chargeOperationDto.Resolution,
                chargeOperationDto.TaxIndicator == TaxIndicator.Tax,
                chargeOperationDto.StartDateTime,
                chargeOperationDto.EndDateTime.GetValueOrDefault());
        }
    }
}
