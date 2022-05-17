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
            var charge = SingleOrDefaultLocal(chargeIdentifier);

            if (charge == null)
            {
                charge = await _chargesDatabaseContext.Charges
                    .SingleAsync(c =>
                        c.SenderProvidedChargeId == chargeIdentifier.SenderProvidedChargeId &&
                        c.OwnerId == chargeIdentifier.Owner &&
                        c.Type == chargeIdentifier.ChargeType)
                    .ConfigureAwait(false);
            }

            return charge;
        }

        public async Task<Charge> SingleAsync(Guid id)
        {
            var charge = SingleOrDefaultLocal(id);
            if (charge == null)
            {
                charge = await _chargesDatabaseContext.Charges
                    .SingleAsync(c =>
                        c.Id == id)
                    .ConfigureAwait(false);
            }

            return charge;
        }

        public async Task<IReadOnlyCollection<Charge>> SingleAsync(IReadOnlyCollection<Guid> ids)
        {
            var charges = new List<Charge>();
            foreach (var id in ids)
            {
                var charge = SingleOrDefaultLocal(id);
                if (charge == null)
                    charges.Add(await SingleAsync(id).ConfigureAwait(false));
            }

            return new ReadOnlyCollection<Charge>(charges);
        }

        public async Task<Charge?> SingleOrNullAsync(ChargeIdentifier chargeIdentifier)
        {
            var charge = SingleOrDefaultLocal(chargeIdentifier);

            if (charge == null)
            {
                charge = await _chargesDatabaseContext.Charges
                    .SingleOrDefaultAsync(c =>
                        c.SenderProvidedChargeId == chargeIdentifier.SenderProvidedChargeId &&
                        c.OwnerId == chargeIdentifier.Owner &&
                        c.Type == chargeIdentifier.ChargeType)
                    .ConfigureAwait(false);
            }

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

        private Charge? SingleOrDefaultLocal(Guid id)
        {
            return _chargesDatabaseContext.Charges
                .Local.SingleOrDefault(c =>
                    c.Id == id);
        }
    }
}
