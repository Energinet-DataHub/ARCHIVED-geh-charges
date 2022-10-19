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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi.QueryServices
{
    public static class QueryServiceAutoMoqDataFixer
    {
        private const string MarketParticipantOwnerId = "MarketParticipantId";

        public static async Task<Guid> GetOrAddMarketParticipantAsync(
            ChargesDatabaseContext context,
            string marketParticipantOwnerId)
        {
            var marketParticipant = await context
                .MarketParticipants
                .SingleOrDefaultAsync(x => x.MarketParticipantId == marketParticipantOwnerId);

            if (marketParticipant != null)
            {
                return marketParticipant.Id;
            }

            marketParticipant = new MarketParticipant(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                marketParticipantOwnerId,
                MarketParticipantStatus.Active,
                MarketParticipantRole.GridAccessProvider);
            context.MarketParticipants.Add(marketParticipant);
            await context.SaveChangesAsync();

            return marketParticipant.Id;
        }

        public static async Task<Guid> GetOrAddMarketParticipantAsync(
            ChargesDatabaseContext context)
        {
            return await GetOrAddMarketParticipantAsync(context, MarketParticipantOwnerId);
        }
    }
}
