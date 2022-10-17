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
using System.Threading.Tasks;
using Energinet.DataHub.Charges.Contracts.Charge;
using GreenEnergyHub.Charges.QueryApi.ModelPredicates;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.QueryApi.QueryServices
{
    public class MarketParticipantQueryService : IMarketParticipantQueryService
    {
        private readonly IData _data;

        public MarketParticipantQueryService(IData data)
        {
            _data = data;
        }

        public async Task<IList<MarketParticipantV1Dto>> GetAsync()
        {
            return await _data.MarketParticipants
                .AsMarketParticipantV1Dto()
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}
