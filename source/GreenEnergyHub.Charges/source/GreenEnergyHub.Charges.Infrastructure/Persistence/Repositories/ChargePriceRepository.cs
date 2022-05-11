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
using GreenEnergyHub.Charges.Domain.ChargePrices;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories
{
    public class ChargePriceRepository : IChargePriceRepository
    {
        private readonly IChargesDatabaseContext _chargesDatabaseContext;

        public ChargePriceRepository(IChargesDatabaseContext chargesDatabaseContext)
        {
            _chargesDatabaseContext = chargesDatabaseContext;
        }

        public async Task AddAsync(ChargePrice chargePrice)
        {
            ArgumentNullException.ThrowIfNull(chargePrice);
            await _chargesDatabaseContext.ChargePrices.AddAsync(chargePrice).ConfigureAwait(false);
        }

        public async Task<ICollection<ChargePrice>> GetOrNullAsync(Guid chargeInformationId, Instant startDate, Instant endDate)
        {
            return await GetChargePriceQueryable(chargeInformationId, startDate, endDate).ToListAsync().ConfigureAwait(false);
        }

        public void RemoveRange(IEnumerable<ChargePrice> chargePrices)
        {
            ArgumentNullException.ThrowIfNull(chargePrices);
            _chargesDatabaseContext.ChargePrices.RemoveRange(chargePrices);
        }

        private IQueryable<ChargePrice> GetChargePriceQueryable(Guid chargeInformationId, Instant startDate, Instant endDate)
        {
            var query =
                from p in _chargesDatabaseContext.ChargePrices
                where p.ChargeInformationId == chargeInformationId
                where p.Time >= startDate
                where p.Time <= endDate
                select p;
            return query;
        }
    }
}
