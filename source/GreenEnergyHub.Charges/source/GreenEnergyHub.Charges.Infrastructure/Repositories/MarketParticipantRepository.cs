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

using System.Linq;
using GreenEnergyHub.Charges.Application.ChangeOfCharges.Repositories;
using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Mapping;

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
                .MarketParticipant
                .Where(mp => mp.MarketParticipantId == id)
                .AsEnumerable()
                .Select(_mapper.ToDomainObject)
                .SingleOrDefault();
        }
    }
}
