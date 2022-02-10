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
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry.Persistence;
using GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry.Persistence.Actors;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry.MarketParticipantsSynchronization
{
    public class MarketParticipantsSynchronizer : IMarketParticipantsSynchronizer
    {
        private readonly IMarketParticipantRegistry _marketParticipantRegistry;
        private readonly IChargesDatabaseContext _chargesDatabaseContext;

        /// <summary>
        /// The roles used in the charges domain.
        /// </summary>
        private readonly List<Role> _rolesUsedInChargesDomain;

        public MarketParticipantsSynchronizer(
            IMarketParticipantRegistry marketParticipantRegistry,
            IChargesDatabaseContext chargesDatabaseContext)
        {
            _marketParticipantRegistry = marketParticipantRegistry;
            _chargesDatabaseContext = chargesDatabaseContext;

            _rolesUsedInChargesDomain = MarketParticipant._validRoles.Select(MarketParticipantRoleMapper.Map).ToList();
        }

        public async Task SynchronizeAsync()
        {
            var marketParticipants = await _chargesDatabaseContext.MarketParticipants.ToListAsync().ConfigureAwait(false);

            var actors = (await _marketParticipantRegistry
                .Actors
                .ToListAsync()
                .ConfigureAwait(false))
                .Where(a => _rolesUsedInChargesDomain.Any(r => a.Roles.Contains(r)))
                .ToList();

            foreach (var actor in actors)
                AddOrUpdateMarketParticipant(marketParticipants, actor);

            await _chargesDatabaseContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private void AddOrUpdateMarketParticipant(List<MarketParticipant> marketParticipants, Actor actor)
        {
            var marketParticipant = marketParticipants.SingleOrDefault(m => m.Id == actor.Id);

            // Add or update market participant. Deletes are not supported (stated by the business).
            if (marketParticipant == null)
            {
                var newMarketParticipant = CreateMarketParticipant(actor);
                _chargesDatabaseContext.MarketParticipants.Add(newMarketParticipant);
            }
            else
            {
                var businessProcessRole = GetBusinessProcessRole(actor.Roles);
                MarketParticipantUpdater.Update(marketParticipant, actor, businessProcessRole);
            }
        }

        private MarketParticipant CreateMarketParticipant(Actor actor)
        {
            var businessProcessRole = GetBusinessProcessRole(actor.Roles);
            return new MarketParticipant(actor.Id, actor.IdentificationNumber, actor.Active, businessProcessRole);
        }

        /// <summary>
        /// Select the single role from the actor roles that the actor must use
        /// with the business processes in the charges domain.
        /// </summary>
        private MarketParticipantRole GetBusinessProcessRole(IEnumerable<Role> roles) =>
            MarketParticipantRoleMapper.Map(roles.Single(r => _rolesUsedInChargesDomain.Contains(r)));
    }
}
