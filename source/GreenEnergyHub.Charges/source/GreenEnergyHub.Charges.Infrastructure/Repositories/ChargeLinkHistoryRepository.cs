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
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkHistory;
using GreenEnergyHub.Charges.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.Repositories
{
    public class ChargeLinkHistoryRepository : IChargeLinkHistoryRepository
    {
        private readonly IChargesDatabaseContext _context;
        private readonly IChargeLinkFactory _chargeLinkFactory;

        public ChargeLinkHistoryRepository(IChargesDatabaseContext context, IChargeLinkFactory chargeLinkFactory)
        {
            _context = context;
            _chargeLinkFactory = chargeLinkFactory;
        }

        public async Task StoreChargeLinkHistoryAsync(ChargeLinkCommandAcceptedEvent chargeLinkCommandAcceptedEvent)
        {
            ChargeLinkHistory chargeLinkHistory =
                _chargeLinkFactory.MapChargeLinkCommandAcceptedEvent(chargeLinkCommandAcceptedEvent);
            await _context.ChargeLinkHistories.AddAsync(chargeLinkHistory);
            await _context.SaveChangesAsync();
        }

        public Task<List<ChargeLinkHistory>> GetChargeHistoriesAsync(IEnumerable<Guid> postOfficeIds)
        {
            var queryable = _context.ChargeLinkHistories.Where(x => postOfficeIds.Contains(x.PostOfficeId));
            return queryable.ToListAsync();
        }
    }
}
