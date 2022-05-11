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
using GreenEnergyHub.Charges.Domain.ChargeInformation;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories
{
    public class ChargeInformationRepository : IChargeInformationRepository
    {
        private readonly IChargesDatabaseContext _chargesDatabaseContext;

        public ChargeInformationRepository(IChargesDatabaseContext chargesDatabaseContext)
        {
            _chargesDatabaseContext = chargesDatabaseContext;
        }

        public Task<ChargeInformation> GetAsync(ChargeInformationIdentifier chargeInformationIdentifier)
        {
            return GetChargeQueryable(chargeInformationIdentifier).SingleAsync();
        }

        public Task<ChargeInformation> GetAsync(Guid id)
        {
            return GetChargesAsQueryable().SingleAsync(x => x.Id == id);
        }

        public async Task<IReadOnlyCollection<ChargeInformation>> GetAsync(IReadOnlyCollection<Guid> ids)
        {
            return await GetChargesAsQueryable()
                .Where(x => ids.Contains(x.Id))
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<ChargeInformation?> GetOrNullAsync(ChargeInformationIdentifier chargeInformationIdentifier)
        {
            return await GetChargeQueryable(chargeInformationIdentifier).SingleOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task AddAsync(ChargeInformation chargeInformation)
        {
            if (chargeInformation == null) throw new ArgumentNullException(nameof(chargeInformation));
            await _chargesDatabaseContext.ChargeInformations.AddAsync(chargeInformation).ConfigureAwait(false);
        }

        private IQueryable<ChargeInformation> GetChargesAsQueryable()
        {
            return _chargesDatabaseContext.ChargeInformations.AsQueryable();
        }

        private IQueryable<ChargeInformation> GetChargeQueryable(ChargeInformationIdentifier chargeInformationIdentifier)
        {
            var query =
                from c in GetChargesAsQueryable()
                join o in _chargesDatabaseContext.MarketParticipants
                    on c.OwnerId equals o.Id
                where c.SenderProvidedChargeId == chargeInformationIdentifier.SenderProvidedChargeId
                where o.MarketParticipantId == chargeInformationIdentifier.Owner
                where c.Type == chargeInformationIdentifier.ChargeType
                select c;
            return query;
        }
    }
}
