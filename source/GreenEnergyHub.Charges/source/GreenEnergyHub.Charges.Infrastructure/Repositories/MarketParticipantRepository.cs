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
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Context.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.Repositories
{
    public class MarketParticipantRepository : IMarketParticipantRepository
    {
        private readonly IChargesDatabaseContext _chargesDatabaseContext;
        private readonly IMarketParticipantMapper _mapper;

        public MarketParticipantRepository(
            IChargesDatabaseContext chargesDatabaseContext,
            IMarketParticipantMapper mapper)
        {
            _chargesDatabaseContext = chargesDatabaseContext;
            _mapper = mapper;
        }

        public MarketParticipant? GetMarketParticipantOrNull(string id)
        {
            return _chargesDatabaseContext
                .MarketParticipants
                .Where(mp => mp.MarketParticipantId == id)
                .AsEnumerable()
                .Select(_mapper.ToDomainObject)
                .SingleOrDefault();
        }

        /// <summary>
        /// This implementation is temp until grid areas and market participants are implemented in their own
        /// domains and integration event are used  to update a query model in the charges domain.
        ///
        /// Later we need to use the metering point ID to find the grid area and then find the responsible market
        /// participant of the grid area.
        /// </summary>
        /// <param name="meteringPointId">ID of the metering point to find the grid access provider for</param>
        /// <returns>The grid access provider responsible for the metering point</returns>
        public MarketParticipant GetGridAccessProvider(string meteringPointId)
        {
            return new MarketParticipant
            {
                Id = "8100000000030",
                BusinessProcessRole = MarketParticipantRole.GridAccessProvider,
            };
        }

        // Later we need to only get the active grid access providers
        public async Task<List<MarketParticipant>> GetActiveGridAccessProvidersAsync()
        {
            var activeGridAccessProviders = await _chargesDatabaseContext.MarketParticipants
                .Where(x => x.Role == (int)MarketParticipantRole.GridAccessProvider).ToListAsync();

            return activeGridAccessProviders.Select(provider =>
                    new MarketParticipant
                        {
                            Id = provider.Id.ToString(),
                            BusinessProcessRole = (MarketParticipantRole)provider.Role,
                        })
                .ToList();
        }
    }
}
