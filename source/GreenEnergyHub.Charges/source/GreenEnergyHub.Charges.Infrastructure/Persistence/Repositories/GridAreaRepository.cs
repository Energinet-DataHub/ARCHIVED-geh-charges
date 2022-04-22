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
using GreenEnergyHub.Charges.Domain.GridAreas;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories
{
    public class GridAreaRepository : IGridAreaRepository
    {
        private readonly IChargesDatabaseContext _chargesDatabaseContext;

        public GridAreaRepository(IChargesDatabaseContext chargesDatabaseContext)
        {
            _chargesDatabaseContext = chargesDatabaseContext;
        }

        public async Task AddAsync(GridArea gridArea)
        {
            if (gridArea is null) throw new ArgumentNullException(nameof(gridArea));
            await _chargesDatabaseContext.GridAreas.AddAsync(gridArea).ConfigureAwait(false);
        }

        public async Task<GridArea?> GetOrNullAsync(Guid id)
        {
            return await _chargesDatabaseContext
                .GridAreas.
                SingleOrDefaultAsync(ga => ga.Id == id).ConfigureAwait(false);
        }
    }
}
