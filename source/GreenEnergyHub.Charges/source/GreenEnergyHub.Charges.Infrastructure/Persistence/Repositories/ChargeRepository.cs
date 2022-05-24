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
using System.Collections.ObjectModel;
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

        public async Task<Charge> SingleAsync(ChargeIdentifier chargeIdentifier)
        {
            var charge = SingleOrDefaultLocal(chargeIdentifier) ??
                         await SingleFromDbAsync(chargeIdentifier).ConfigureAwait(false);
            return charge;
        }

        public async Task<IReadOnlyCollection<Charge>> GetByIdsAsync(IReadOnlyCollection<Guid> ids)
        {
            var charges = new List<Charge>();

            var chargesFoundInLocalContext = _chargesDatabaseContext.Charges.Local.Where(charge => ids.Contains(charge.Id)).ToList();
            var chargeIdsFoundInLocalContext = chargesFoundInLocalContext.Select(charge => charge.Id).ToList();

            var chargeIdsNotFoundInLocalContext = ids.Except(chargeIdsFoundInLocalContext);
            var chargesFetchedFromDatabase = await _chargesDatabaseContext.Charges
                .Where(charge => chargeIdsNotFoundInLocalContext.Contains(charge.Id)).ToListAsync().ConfigureAwait(false);

            charges.AddRange(chargesFoundInLocalContext);
            charges.AddRange(chargesFetchedFromDatabase);

            return new ReadOnlyCollection<Charge>(charges);
        }

        public async Task<Charge?> SingleOrNullAsync(ChargeIdentifier chargeIdentifier)
        {
            var charge = SingleOrDefaultLocal(chargeIdentifier) ??
                         await SingleOrNullFromDbAsync(chargeIdentifier).ConfigureAwait(false);
            return charge;
        }

        public async Task AddAsync(Charge charge)
        {
            if (charge == null) throw new ArgumentNullException(nameof(charge));
            await _chargesDatabaseContext.Charges.AddAsync(charge).ConfigureAwait(false);
        }

        private Charge? SingleOrDefaultLocal(ChargeIdentifier chargeIdentifier)
        {
            return _chargesDatabaseContext.Charges
                .Local.SingleOrDefault(c =>
                    c.SenderProvidedChargeId == chargeIdentifier.SenderProvidedChargeId &&
                    c.OwnerId == chargeIdentifier.Owner &&
                    c.Type == chargeIdentifier.ChargeType);
        }

        private async Task<Charge> SingleFromDbAsync(ChargeIdentifier chargeIdentifier)
        {
            return await _chargesDatabaseContext.Charges
                .SingleAsync(c =>
                    c.SenderProvidedChargeId == chargeIdentifier.SenderProvidedChargeId &&
                    c.OwnerId == chargeIdentifier.Owner &&
                    c.Type == chargeIdentifier.ChargeType)
                .ConfigureAwait(false);
        }

        private async Task<Charge?> SingleOrNullFromDbAsync(ChargeIdentifier chargeIdentifier)
        {
            return await _chargesDatabaseContext.Charges
                .SingleOrDefaultAsync(c =>
                    c.SenderProvidedChargeId == chargeIdentifier.SenderProvidedChargeId &&
                    c.OwnerId == chargeIdentifier.Owner &&
                    c.Type == chargeIdentifier.ChargeType)
                .ConfigureAwait(false);
        }
    }
}
