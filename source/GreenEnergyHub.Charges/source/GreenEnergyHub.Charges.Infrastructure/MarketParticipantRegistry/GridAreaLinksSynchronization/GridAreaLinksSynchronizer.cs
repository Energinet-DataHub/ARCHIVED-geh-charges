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
using GreenEnergyHub.Charges.Domain.GridAreaLinks;
using GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry.Persistence;
using GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry.Persistence.GridAreas;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry.GridAreaLinksSynchronization
{
    public class GridAreaLinksSynchronizer : IGridAreaLinksSynchronizer
    {
        private readonly IMarketParticipantRegistry _marketParticipantRegistry;
        private readonly IChargesDatabaseContext _chargesDatabaseContext;

        public GridAreaLinksSynchronizer(
            IMarketParticipantRegistry marketParticipantRegistry,
            IChargesDatabaseContext chargesDatabaseContext)
        {
            _marketParticipantRegistry = marketParticipantRegistry;
            _chargesDatabaseContext = chargesDatabaseContext;
        }

        public async Task SynchronizeAsync()
        {
            var areasInRegister = await _marketParticipantRegistry.GridAreas.ToListAsync().ConfigureAwait(false);
            var linksInRegister = await _marketParticipantRegistry.GridAreaLinks.ToListAsync().ConfigureAwait(false);
            var links = await _chargesDatabaseContext.GridAreaLinks.ToListAsync().ConfigureAwait(false);

            foreach (var linkInRegister in linksInRegister)
                await AddOrUpdateGridAreaLinkAsync(links, linkInRegister, areasInRegister).ConfigureAwait(false);

            await _chargesDatabaseContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task AddOrUpdateGridAreaLinkAsync(
            List<GridAreaLink> links,
            Persistence.GridAreaLinks.GridAreaLink linkInRegister,
            List<GridArea> areasInRegister)
        {
            var link = links.SingleOrDefault(a => a.Id == linkInRegister.Id);
            var gridAreaInRegister = areasInRegister.Single(a => a.Id == linkInRegister.GridAreaId);
            if (link == null)
                await AddGridAreaLinkAsync(linkInRegister, gridAreaInRegister).ConfigureAwait(false);
            else
                UpdateGridAreaLink(linkInRegister, link, gridAreaInRegister);
        }

        private static void UpdateGridAreaLink(
            Persistence.GridAreaLinks.GridAreaLink linkInRegister,
            GridAreaLink link,
            GridArea gridAreaInRegister)
        {
            link.GridAreaId = linkInRegister.GridAreaId;
            link.OwnerId = gridAreaInRegister.ActorId;
        }

        private async Task AddGridAreaLinkAsync(
            Persistence.GridAreaLinks.GridAreaLink linkInRegister,
            GridArea gridAreaInRegister)
        {
            var link = new GridAreaLink(linkInRegister.Id, linkInRegister.GridAreaId, gridAreaInRegister.ActorId);
            await _chargesDatabaseContext.GridAreaLinks.AddAsync(link).ConfigureAwait(false);
        }
    }
}
