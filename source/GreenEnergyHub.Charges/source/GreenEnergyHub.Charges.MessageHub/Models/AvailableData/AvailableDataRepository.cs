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
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableData
{
    public class AvailableDataRepository<TAvailableData> : IAvailableDataRepository<TAvailableData>
        where TAvailableData : AvailableDataBase
    {
        private readonly IMessageHubDatabaseContext _context;

        public AvailableDataRepository(IMessageHubDatabaseContext messageHubDatabaseContext)
        {
            _context = messageHubDatabaseContext;
        }

        public async Task StoreAsync(IEnumerable<TAvailableData> availableData)
        {
            await _context.SetAsync<TAvailableData>().AddRangeAsync(availableData).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<TAvailableData>> GetAsync(IEnumerable<Guid> dataReferenceIds)
        {
            return await _context
                .SetAsync<TAvailableData>()
                .Where(x => dataReferenceIds.Contains(x.AvailableDataReferenceId))
                .OrderBy(x => x.RequestDateTime)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}
