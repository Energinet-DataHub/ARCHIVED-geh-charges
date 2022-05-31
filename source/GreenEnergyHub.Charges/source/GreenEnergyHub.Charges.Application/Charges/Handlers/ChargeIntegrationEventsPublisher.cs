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
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeIntegrationEventsPublisher : IChargeIntegrationEventsPublisher
    {
        private readonly IChargePublisher _chargePublisher;
        private readonly IChargePricesUpdatedPublisher _chargePricesUpdatedPublisher;

        public ChargeIntegrationEventsPublisher(
            IChargePublisher chargePublisher,
            IChargePricesUpdatedPublisher chargePricesUpdatedPublisher)
        {
            _chargePublisher = chargePublisher;
            _chargePricesUpdatedPublisher = chargePricesUpdatedPublisher;
        }

        public async Task PublishAsync(ChargeCommandAcceptedEvent chargeCommandAcceptedEvent)
        {
            ArgumentNullException.ThrowIfNull(chargeCommandAcceptedEvent);

            foreach (var chargeOperationDto in chargeCommandAcceptedEvent.Command.ChargeOperations)
            {
                switch (chargeOperationDto)
                {
                    case ChargeInformationDto chargeInformationDto:
                        await _chargePublisher.PublishChargeCreatedAsync(chargeInformationDto).ConfigureAwait(false);
                        break;
                    case ChargePriceDto chargePriceDto:
                        await _chargePricesUpdatedPublisher.PublishChargePricesAsync(chargePriceDto).ConfigureAwait(false);
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Operation must be {nameof(ChargeInformationDto)} or {nameof(ChargePriceDto)}");
                }
            }
        }
    }
}
