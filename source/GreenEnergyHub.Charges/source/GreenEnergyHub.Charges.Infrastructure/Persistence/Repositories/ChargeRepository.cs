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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Charges;
using Microsoft.EntityFrameworkCore;
using Charge = GreenEnergyHub.Charges.Domain.Charges.Charge;

namespace GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories
{
    public class ChargeRepository : IChargeRepository
    {
        private readonly IChargesDatabaseContext _chargesDatabaseContext;

        public ChargeRepository(IChargesDatabaseContext chargesDatabaseContext)
        {
            _chargesDatabaseContext = chargesDatabaseContext;
        }

        public Task<Charge> GetAsync(ChargeIdentifier chargeIdentifier)
        {
            return GetChargeQueryable(chargeIdentifier).SingleAsync();
        }

        public Task<Charge> GetAsync(Guid id)
        {
            return GetChargesAsQueryable()
                .SingleAsync(x => x.Id == id);
        }

        public async Task<IReadOnlyCollection<Charge>> GetAsync(IReadOnlyCollection<Guid> ids)
        {
            return await GetChargesAsQueryable()
                .Where(x => ids.Contains(x.Id))
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<Charge?> GetOrNullAsync(ChargeIdentifier chargeIdentifier)
        {
            return await GetChargeQueryable(chargeIdentifier).SingleOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task StoreChargeAsync(Charge charge)
        {
            if (charge == null) throw new ArgumentNullException(nameof(charge));
            await _chargesDatabaseContext.Charges.AddAsync(charge).ConfigureAwait(false);
            await _chargesDatabaseContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private IQueryable<Charge> GetChargesAsQueryable()
        {
            return _chargesDatabaseContext.Charges
                .Include(x => x.Points)
                .AsQueryable();
        }

        private IQueryable<Charge> GetChargeQueryable(ChargeIdentifier chargeIdentifier)
        {
            var query =
                from c in GetChargesAsQueryable()
                join o in _chargesDatabaseContext.MarketParticipants
                    on c.OwnerId equals o.Id
                where c.SenderProvidedChargeId == chargeIdentifier.SenderProvidedChargeId
                where o.MarketParticipantId == chargeIdentifier.Owner
                where c.Type == chargeIdentifier.ChargeType
                select c;
            return query;
        }
    }
}
