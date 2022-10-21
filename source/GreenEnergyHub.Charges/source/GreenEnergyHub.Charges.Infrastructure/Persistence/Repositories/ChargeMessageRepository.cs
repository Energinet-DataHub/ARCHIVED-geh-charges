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
using GreenEnergyHub.Charges.Domain.Charges;

namespace GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories
{
    public class ChargeMessageRepository : IChargeMessageRepository
    {
        private readonly IChargesDatabaseContext _chargesDatabaseContext;

        public ChargeMessageRepository(IChargesDatabaseContext chargesDatabaseContext)
        {
            _chargesDatabaseContext = chargesDatabaseContext;
        }

        public async Task AddAsync(ChargeMessage chargeMessage)
        {
            ArgumentNullException.ThrowIfNull(chargeMessage);

            await _chargesDatabaseContext.ChargeMessages.AddAsync(chargeMessage).ConfigureAwait(false);
        }

        // Todo: only Get in query model
        /*public async Task<IReadOnlyCollection<ChargeMessage>> GetByChargeIdAsync(Guid chargeId)
        {
            var chargeMessages = await _chargesDatabaseContext.ChargeMessages
                .Where(chargeMessage => chargeMessage.ChargeId == chargeId)
                .ToListAsync().ConfigureAwait(false);

            return new ReadOnlyCollection<ChargeMessage>(chargeMessages);
        }*/
    }
}
