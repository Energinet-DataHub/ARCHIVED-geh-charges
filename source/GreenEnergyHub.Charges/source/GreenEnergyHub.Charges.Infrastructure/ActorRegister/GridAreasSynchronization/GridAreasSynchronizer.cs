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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Infrastructure.ActorRegister.Persistence;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.ActorRegister.GridAreasSynchronization
{
    public class GridAreasSynchronizer : IGridAreasSynchronizer
    {
        private readonly IActorRegister _actorRegister;
        private readonly IChargesDatabaseContext _chargesDatabaseContext;

        public GridAreasSynchronizer(IActorRegister actorRegister, IChargesDatabaseContext chargesDatabaseContext)
        {
            _actorRegister = actorRegister;
            _chargesDatabaseContext = chargesDatabaseContext;
        }

        public async Task SynchronizeAsync()
        {
            var areasInRegister = await _actorRegister.GridAreas.ToListAsync().ConfigureAwait(false);
            var areas = await _chargesDatabaseContext.GridAreas.ToListAsync().ConfigureAwait(false);

            foreach (var areaInRegister in areasInRegister)
                await AddOrUpdateGridAreaAsync(areas, areaInRegister);

            await _chargesDatabaseContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task AddOrUpdateGridAreaAsync(List<GridArea> areas, Persistence.GridAreas.GridArea areaInRegister)
        {
            var area = areas.SingleOrDefault(a => a.Id == areaInRegister.Id);

            if (area == null)
                await AddGridAreaAsync(areaInRegister);
            else
                UpdateGridArea(areaInRegister, area);
        }

        private static void UpdateGridArea(Persistence.GridAreas.GridArea areaInRegister, GridArea area)
        {
            area.GridAccessProviderId = areaInRegister.ActorId;
        }

        private async Task AddGridAreaAsync(Persistence.GridAreas.GridArea areaInRegister)
        {
            var area = new GridArea(areaInRegister.Id, areaInRegister.ActorId);
            await _chargesDatabaseContext.GridAreas.AddAsync(area).ConfigureAwait(false);
        }
    }
}
