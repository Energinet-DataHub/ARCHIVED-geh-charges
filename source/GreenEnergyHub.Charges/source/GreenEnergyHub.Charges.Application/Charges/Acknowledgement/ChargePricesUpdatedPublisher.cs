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

using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Domain.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Charges.Acknowledgements;

namespace GreenEnergyHub.Charges.Application.Charges.Acknowledgement
{
    public class ChargePricesUpdatedPublisher : IChargePricesUpdatedPublisher
    {
        private readonly IMessageDispatcher<ChargePricesUpdated> _messagePricesDispatcher;
        private readonly IChargePricesUpdatedFactory _chargePricesUpdatedFactory;

        public ChargePricesUpdatedPublisher(
            IMessageDispatcher<ChargePricesUpdated> messagePricesDispatcher,
            IChargePricesUpdatedFactory chargePricesUpdatedFactory)
        {
            _messagePricesDispatcher = messagePricesDispatcher;
            _chargePricesUpdatedFactory = chargePricesUpdatedFactory;
        }

        public async Task SendChargePricesAsync(ChargeCommandAcceptedEvent chargeCommandAcceptedEvent)
        {
            var prices = _chargePricesUpdatedFactory.Create(chargeCommandAcceptedEvent);
            await _messagePricesDispatcher.DispatchAsync(prices).ConfigureAwait(false);
        }
    }
}
