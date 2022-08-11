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

using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Infrastructure.Outbox;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories
{
    public class OutboxMessageRepository : IOutboxMessageRepository
    {
        private readonly IChargesDatabaseContext _chargesDatabaseContext;
        private readonly IClock _clock;

        public OutboxMessageRepository(
            IChargesDatabaseContext chargesDatabaseContext,
            IClock clock)
        {
            _chargesDatabaseContext = chargesDatabaseContext;
            _clock = clock;
        }

        public void Add(OutboxMessage outboxMessage)
        {
            _chargesDatabaseContext.OutboxMessages.Add(outboxMessage);
        }

        public OutboxMessage? GetNext()
        {
            return _chargesDatabaseContext.OutboxMessages
                .OrderBy(message => message.CreationDate)
                .FirstOrDefault(message => !message.ProcessedDate.HasValue);
        }

        public Task MarkProcessedAsync(OutboxMessage outboxMessage)
        {
            var processedMessage = _chargesDatabaseContext.OutboxMessages.Single(message => message.Id == outboxMessage.Id);
            processedMessage.SetProcessed(_clock.GetCurrentInstant());
            return _chargesDatabaseContext.SaveChangesAsync();
        }
    }
}
