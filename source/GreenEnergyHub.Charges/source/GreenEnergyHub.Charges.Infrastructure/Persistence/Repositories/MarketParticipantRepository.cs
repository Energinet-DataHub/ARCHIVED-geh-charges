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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
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

        /// <summary>
        /// Persist a market participant
        /// </summary>
        /// <param name="marketParticipant"></param>
        public async Task AddAsync(MarketParticipant marketParticipant)
        {
            ArgumentNullException.ThrowIfNull(marketParticipant);
            await _chargesDatabaseContext.MarketParticipants.AddAsync(marketParticipant).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves a market participant from actorId
        /// </summary>
        /// <param name="actorId"></param>
        public async Task<MarketParticipant?> GetByActorIdAsync(Guid? actorId)
        {
            return await _chargesDatabaseContext
                .MarketParticipants
                .SingleOrDefaultAsync(mp => mp.ActorId == actorId).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves a market participant from b2CActorId
        /// </summary>
        /// <param name="b2CActorId"></param>
        public async Task<MarketParticipant?> SingleOrNullAsync(Guid? b2CActorId)
        {
            return await _chargesDatabaseContext
                .MarketParticipants
                .SingleOrDefaultAsync(mp => mp.B2CActorId == b2CActorId).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves a market participant from gln/eic no.
        /// </summary>
        /// <param name="marketParticipantId"></param>
        public async Task<MarketParticipant?> SingleOrNullAsync(string marketParticipantId)
        {
            var roles = new HashSet<MarketParticipantRole>
            {
                MarketParticipantRole.SystemOperator, MarketParticipantRole.GridAccessProvider,
            };

            return await _chargesDatabaseContext
                .MarketParticipants
                .SingleOrDefaultAsync(mp =>
                    mp.MarketParticipantId == marketParticipantId &&
                    roles.Contains(mp.BusinessProcessRole))
                .ConfigureAwait(false);
        }

        public Task<List<MarketParticipant>> GetActiveEnergySuppliersAsync()
        {
            return _chargesDatabaseContext
                .MarketParticipants
                .Where(mp => mp.BusinessProcessRole == MarketParticipantRole.EnergySupplier)
                .Where(m =>
                    m.Status == MarketParticipantStatus.Active ||
                    m.Status == MarketParticipantStatus.Passive)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a market participant from role and gln/eic no.
        /// </summary>
        /// <param name="businessProcessRole"></param>
        /// <param name="marketParticipantId"></param>
        public async Task<MarketParticipant?> SingleOrNullAsync(
            MarketParticipantRole businessProcessRole,
            string marketParticipantId)
        {
            return await _chargesDatabaseContext
                .MarketParticipants
                .SingleOrDefaultAsync(mp =>
                    mp.MarketParticipantId == marketParticipantId &&
                    mp.BusinessProcessRole == businessProcessRole)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<MarketParticipant>> GetAsync(IEnumerable<Guid> ids)
        {
            return await _chargesDatabaseContext
                .MarketParticipants
                .Where(mp => ids.Contains(mp.Id))
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public Task<List<MarketParticipant>> GetActiveGridAccessProvidersAsync()
        {
            return _chargesDatabaseContext
                .MarketParticipants
                .Where(mp => mp.BusinessProcessRole == MarketParticipantRole.GridAccessProvider)
                .Where(m =>
                    m.Status == MarketParticipantStatus.Active ||
                    m.Status == MarketParticipantStatus.Passive)
                .ToListAsync();
        }

        public Task<MarketParticipant> GetGridAccessProviderAsync(string meteringPointId)
        {
            ArgumentNullException.ThrowIfNull(meteringPointId);
            if (string.IsNullOrEmpty(meteringPointId.Trim())) throw new ArgumentException();

            // According to product owner the business processes should not be able to result
            // in encountering an inactive grid area nor a grid area without
            // an owner grid access provider. So no special handling of those cases.
            return (from meteringPoint in _chargesDatabaseContext.MeteringPoints
                    from gridAreaLink in _chargesDatabaseContext.GridAreaLinks
                    from marketParticipant in _chargesDatabaseContext.MarketParticipants
                    where gridAreaLink.OwnerId == marketParticipant.Id
                    where meteringPoint.MeteringPointId == meteringPointId
                    where meteringPoint.GridAreaLinkId == gridAreaLink.Id
                    select marketParticipant)
                .SingleAsync();
        }

        public Task<MarketParticipant?> GetGridAccessProviderAsync(Guid gridAreaId)
        {
            return (from gridAreaLink in _chargesDatabaseContext.GridAreaLinks
                    from marketParticipant in _chargesDatabaseContext.MarketParticipants
                    where gridAreaLink.OwnerId == marketParticipant.Id
                    where gridAreaLink.GridAreaId == gridAreaId
                    select marketParticipant)
                .SingleOrDefaultAsync();
        }

        public Task<MarketParticipant> GetMeteringPointAdministratorAsync()
        {
            return SingleAsync(MarketParticipantRole.MeteringPointAdministrator);
        }

        public Task<MarketParticipant> GetSystemOperatorAsync()
        {
            return SingleAsync(MarketParticipantRole.SystemOperator);
        }

        /// <summary>
        /// Retrieves an active market participant from gln/eic no. with the role EZ or DDM
        /// </summary>
        /// <param name="marketParticipantId"></param>
        public async Task<MarketParticipant> GetSystemOperatorOrGridAccessProviderAsync(string marketParticipantId)
        {
            var roles = new HashSet<MarketParticipantRole>
            {
                MarketParticipantRole.SystemOperator, MarketParticipantRole.GridAccessProvider,
            };

            return await _chargesDatabaseContext
                .MarketParticipants
                .SingleAsync(mp =>
                    mp.MarketParticipantId == marketParticipantId &&
                    (mp.Status == MarketParticipantStatus.Active || mp.Status == MarketParticipantStatus.Passive) &&
                    roles.Contains(mp.BusinessProcessRole))
                .ConfigureAwait(false);
        }

        private Task<MarketParticipant> SingleAsync(MarketParticipantRole marketParticipantRole)
        {
            return _chargesDatabaseContext
                .MarketParticipants
                .Where(mp => mp.BusinessProcessRole == marketParticipantRole &&
                             (mp.Status == MarketParticipantStatus.Active || mp.Status == MarketParticipantStatus.Passive))
                .SingleAsync();
        }
    }
}
