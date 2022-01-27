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
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories
{
    public class MarketParticipantRepository : IMarketParticipantRepository
    {
        private readonly IChargesDatabaseContext _chargesDatabaseContext;

        public MarketParticipantRepository(
            IChargesDatabaseContext chargesDatabaseContext)
        {
            _chargesDatabaseContext = chargesDatabaseContext;
        }

        public Task<MarketParticipant> GetOrNullAsync(string marketParticipantId)
        {
            return _chargesDatabaseContext
                .MarketParticipants
                .SingleOrDefaultAsync(mp => mp.MarketParticipantId == marketParticipantId);
        }

        public Task<List<MarketParticipant>> GetGridAccessProvidersAsync()
        {
            return _chargesDatabaseContext
                .MarketParticipants
                .Where(mp => mp.BusinessProcessRole == MarketParticipantRole.GridAccessProvider)
                .Where(m => m.IsActive)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<MarketParticipant>> GetAsync(IEnumerable<Guid> ids)
        {
            return await _chargesDatabaseContext
                .MarketParticipants
                .Where(mp => ids.Contains(mp.Id))
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public Task<MarketParticipant> GetGridAccessProviderAsync(string meteringPointId)
        {
            if (meteringPointId == null) throw new ArgumentNullException(nameof(meteringPointId));
            if (string.IsNullOrEmpty(meteringPointId.Trim())) throw new ArgumentException();

            // According to product owner the business processes should not be able to result
            // in encountering an inactive grid area nor a grid area without
            // an owner grid access provider. So no special handling of those cases.
            return (from meteringPoint in _chargesDatabaseContext.MeteringPoints
                    from gridAreaLink in _chargesDatabaseContext.GridAreaLinks
                    from gridArea in _chargesDatabaseContext.GridAreas
                    from marketParticipant in _chargesDatabaseContext.MarketParticipants
                    where gridArea.GridAccessProviderId == marketParticipant.Id
                    where meteringPoint.MeteringPointId == meteringPointId
                    where meteringPoint.GridAreaLinkId == gridAreaLink.Id
                    where gridAreaLink.GridAreaId == gridArea.Id
                    select marketParticipant)
                .SingleAsync();
        }

        public Task<MarketParticipant> GetMeteringPointAdministratorAsync()
        {
            return GetAsync(MarketParticipantRole.MeteringPointAdministrator);
        }

        public Task<MarketParticipant> GetSystemOperatorAsync()
        {
            return GetAsync(MarketParticipantRole.SystemOperator);
        }

        private Task<MarketParticipant> GetAsync(MarketParticipantRole marketParticipantRole)
        {
            return _chargesDatabaseContext
                .MarketParticipants
                .Where(mp => mp.BusinessProcessRole == marketParticipantRole)
                .Where(mp => mp.IsActive)
                .SingleAsync();
        }
    }
}
