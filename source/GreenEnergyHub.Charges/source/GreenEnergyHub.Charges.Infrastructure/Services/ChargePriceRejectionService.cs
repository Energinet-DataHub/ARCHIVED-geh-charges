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

using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Application.Charges.Services;
using GreenEnergyHub.Charges.Infrastructure.Outbox;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;

namespace GreenEnergyHub.Charges.Infrastructure.Services
{
    public class ChargePriceRejectionService : IChargePriceRejectionService
    {
        private readonly IOutboxMessageRepository _outboxMessageRepository;
        private readonly OutboxMessageFactory _outboxMessageFactory;

        public ChargePriceRejectionService(
            IOutboxMessageRepository outboxMessageRepository,
            OutboxMessageFactory outboxMessageFactory)
        {
            _outboxMessageRepository = outboxMessageRepository;
            _outboxMessageFactory = outboxMessageFactory;
        }

        public void SaveRejections(ChargePriceOperationsRejectedEvent chargePriceOperationsRejectedEvent)
        {
            var outboxMessage = _outboxMessageFactory.CreateFrom(chargePriceOperationsRejectedEvent);
            _outboxMessageRepository.Add(outboxMessage);
        }
    }
}
