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

using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Core;
using GreenEnergyHub.Charges.Core.Currency;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;

namespace GreenEnergyHub.Charges.Application.Charges.Factories
{
    public class ChargeCreatedEventFactory : IChargeCreatedEventFactory
    {
        private readonly CurrencyConfigurationIso4217 _currencyConfigurationIso4217;

        public ChargeCreatedEventFactory(CurrencyConfigurationIso4217 currencyConfigurationIso4217)
        {
            _currencyConfigurationIso4217 = currencyConfigurationIso4217;
        }

        public ChargeCreatedEvent Create([NotNull] ChargeCommandAcceptedEvent chargeCommandAcceptedEvent)
        {
            return new ChargeCreatedEvent(
                chargeCommandAcceptedEvent.Command.ChargeOperation.ChargeId,
                chargeCommandAcceptedEvent.Command.ChargeOperation.Type,
                chargeCommandAcceptedEvent.Command.ChargeOperation.ChargeOwner,
                _currencyConfigurationIso4217.Currency,
                chargeCommandAcceptedEvent.Command.ChargeOperation.Resolution,
                chargeCommandAcceptedEvent.Command.ChargeOperation.TaxIndicator,
                chargeCommandAcceptedEvent.Command.ChargeOperation.StartDateTime,
                chargeCommandAcceptedEvent.Command.ChargeOperation.EndDateTime.GetValueOrDefault());
        }
    }
}
