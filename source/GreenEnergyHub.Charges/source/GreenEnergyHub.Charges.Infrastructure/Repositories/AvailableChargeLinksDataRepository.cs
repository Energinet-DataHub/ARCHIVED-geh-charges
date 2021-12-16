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
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Domain.AvailableData;
using GreenEnergyHub.Charges.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.Repositories
{
    public class AvailableChargeLinksDataRepository : IAvailableDataRepository<AvailableChargeLinksData>
    {
        private readonly IChargesDatabaseContext _context;

        public AvailableChargeLinksDataRepository(IChargesDatabaseContext context)
        {
            _context = context;
        }

        public async Task StoreAsync(IEnumerable<AvailableChargeLinksData> availableChargeLinksData)
        {
            await _context.AvailableChargeLinksData.AddRangeAsync(availableChargeLinksData);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<AvailableChargeLinksData>> GetAsync(IEnumerable<Guid> dataReferenceIds)
        {
            var queryable = _context.AvailableChargeLinksData.Where(x => dataReferenceIds.Contains(x.AvailableDataReferenceId));
            return await queryable
                .OrderBy(x => x.RequestDateTime)
                .ToListAsync();
        }
    }
}
