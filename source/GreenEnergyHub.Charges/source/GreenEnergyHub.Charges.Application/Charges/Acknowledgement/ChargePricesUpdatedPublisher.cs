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
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;

namespace GreenEnergyHub.Charges.Application.Charges.Acknowledgement
{
    public class ChargePricesUpdatedPublisher : IChargePricesUpdatedPublisher
    {
        private readonly IMessageDispatcher<ChargePricesUpdatedEvent> _messagePricesDispatcher;
        private readonly IChargePricesUpdatedEventFactory _chargePricesUpdatedEventFactory;

        public ChargePricesUpdatedPublisher(
            IMessageDispatcher<ChargePricesUpdatedEvent> messagePricesDispatcher,
            IChargePricesUpdatedEventFactory chargePricesUpdatedEventFactory)
        {
            _messagePricesDispatcher = messagePricesDispatcher;
            _chargePricesUpdatedEventFactory = chargePricesUpdatedEventFactory;
        }

        public async Task PublishChargePricesAsync(ChargeOperationDto chargeOperationDto)
        {
            var prices = _chargePricesUpdatedEventFactory.Create(chargeOperationDto);
            await _messagePricesDispatcher.DispatchAsync(prices).ConfigureAwait(false);
        }
    }
}
