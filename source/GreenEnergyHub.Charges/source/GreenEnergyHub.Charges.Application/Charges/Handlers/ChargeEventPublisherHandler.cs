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
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Charges.Acknowledgements;
using GreenEnergyHub.Charges.Domain.Charges.Events.Local;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeEventPublisherHandler
    {
        private readonly IMessageDispatcher<ChargeCreated> _messageChargeDispatcher;
        private readonly IMessageDispatcher<ChargePricesUpdated> _messagePricesDispatcher;

        public ChargeEventPublisherHandler(
            IMessageDispatcher<ChargeCreated> messageChargeDispatcher,
            IMessageDispatcher<ChargePricesUpdated> messagePricesDispatcher)
        {
            _messagePricesDispatcher = messagePricesDispatcher;
            _messageChargeDispatcher = messageChargeDispatcher;
        }

        public async Task HandleAsync(ChargeCommandAcceptedEvent chargeCommandAcceptedEvent)
        {
            if (chargeCommandAcceptedEvent == null) throw new ArgumentNullException(nameof(chargeCommandAcceptedEvent));

            if (chargeCommandAcceptedEvent.Command.ChargeOperation.Points.Any())
            {
                await _messageChargeDispatcher.DispatchAsync(null!).ConfigureAwait(false);
                await _messagePricesDispatcher.DispatchAsync(null!).ConfigureAwait(false);
            }
            else
            {
                await _messageChargeDispatcher.DispatchAsync(null!).ConfigureAwait(false);
            }
        }
    }
}
