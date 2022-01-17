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
using GreenEnergyHub.Charges.Infrastructure.ActorRegister.Persistence;
using GreenEnergyHub.Charges.Infrastructure.ActorRegister.Persistence.Actors;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.ActorRegister
{
    public class MarketParticipantSynchronizer : IMarketParticipantSynchronizer
    {
        private readonly IActorRegister _actorRegister;
        private readonly IChargesDatabaseContext _chargesDatabaseContext;

        /// <summary>
        /// The roles used in the charges domain.
        /// </summary>
        private readonly List<Role> _rolesUsedInChargesDomain;

        public MarketParticipantSynchronizer(
            IActorRegister actorRegister,
            IChargesDatabaseContext chargesDatabaseContext)
        {
            _actorRegister = actorRegister;
            _chargesDatabaseContext = chargesDatabaseContext;

            _rolesUsedInChargesDomain = MarketParticipant.ValidRoles.Select(MarketParticipantRoleMapper.Map).ToList();
        }

        public async Task SynchronizeAsync()
        {
            var marketParticipants = await _chargesDatabaseContext.MarketParticipants.ToListAsync();

            var actors = (await _actorRegister
                .Actors
                .ToListAsync())
                .Where(a => _rolesUsedInChargesDomain.Any(r => a.Roles.Contains(r)))
                .ToList();

            foreach (var actor in actors)
            {
                var marketParticipant = marketParticipants.SingleOrDefault(m => m.Id == actor.Id);

                // Add or update market participant. Deletes are not supported.
                if (marketParticipant == null)
                {
                    // Remove if (anticipated) test data conflicts with data from actor register.
                    // We consider the ID to be the UUID identifier of the actor and not the identification number.
                    // This is supported by the fact that the ID is also used for authentication.
                    var testData = marketParticipants
                        .SingleOrDefault(mp => mp.MarketParticipantId == actor.IdentificationNumber);
                    if (testData != null)
                    {
                        _chargesDatabaseContext.MarketParticipants.Remove(testData);

                        // It's necessary to save changes in order to be able to add market participant
                        // with same identification number without violating the unique constraint of the database
                        await _chargesDatabaseContext.SaveChangesAsync();
                    }

                    var newMarketParticipant = CreateMarketParticipant(actor);
                    _chargesDatabaseContext.MarketParticipants.Add(newMarketParticipant);
                }
                else
                {
                    var businessProcessRole = GetBusinessProcessRole(actor.Roles);
                    MarketParticipantUpdater.Update(marketParticipant, actor, businessProcessRole);
                }
            }

            await _chargesDatabaseContext.SaveChangesAsync();
        }

        private MarketParticipant CreateMarketParticipant(Actor actor)
        {
            var businessProcessRole = GetBusinessProcessRole(actor.Roles);
            var businessProcessRoles = new List<MarketParticipantRole> { businessProcessRole };
            return new MarketParticipant(actor.Id, actor.IdentificationNumber, actor.Active, businessProcessRoles);
        }

        /// <summary>
        /// Select the single role from the actor roles that the actor must use
        /// with the business processes in the charges domain.
        /// </summary>
        private MarketParticipantRole GetBusinessProcessRole(IEnumerable<Role> roles) =>
            MarketParticipantRoleMapper.Map(roles.Single(r => _rolesUsedInChargesDomain.Contains(r)));
    }
}
