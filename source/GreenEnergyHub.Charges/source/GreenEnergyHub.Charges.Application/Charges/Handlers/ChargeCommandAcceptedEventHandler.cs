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
using GreenEnergyHub.Charges.Domain.ChargeCommandAcceptedEvents;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeCommandAcceptedEventHandler : IChargeCommandAcceptedEventHandler
    {
        private readonly IChargeSender _chargeSender;
        private readonly IChargePricesUpdatedSender _chargePricesUpdatedSender;

        public ChargeCommandAcceptedEventHandler(
            IChargeSender chargeSender,
            IChargePricesUpdatedSender chargePricesUpdatedSender)
        {
            _chargeSender = chargeSender;
            _chargePricesUpdatedSender = chargePricesUpdatedSender;
        }

        public async Task HandleAsync(ChargeCommandAcceptedEvent chargeCommandAcceptedEvent)
        {
            if (chargeCommandAcceptedEvent == null) throw new ArgumentNullException(nameof(chargeCommandAcceptedEvent));

            await _chargeSender
                .SendChargeCreatedAsync(chargeCommandAcceptedEvent)
                .ConfigureAwait(false);

            if (chargeCommandAcceptedEvent.HasPrices())
            {
                await _chargePricesUpdatedSender
                    .SendChargePricesAsync(chargeCommandAcceptedEvent)
                    .ConfigureAwait(false);
            }
        }
    }
}
