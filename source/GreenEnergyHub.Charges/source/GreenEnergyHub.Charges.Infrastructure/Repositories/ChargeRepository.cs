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
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChangeOfCharges.Repositories;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using GreenEnergyHub.Charges.Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;
using Charge = GreenEnergyHub.Charges.Domain.Charge;

namespace GreenEnergyHub.Charges.Infrastructure.Repositories
{
    public class ChargeRepository : IChargeRepository
    {
        private readonly IChargesDatabaseContext _chargesDatabaseContext;

        public ChargeRepository(IChargesDatabaseContext chargesDatabaseContext)
        {
            _chargesDatabaseContext = chargesDatabaseContext;
        }

        public async Task<Charge> GetChargeAsync(string chargeId, string owner, ChargeType chargeType)
        {
            var charge = await _chargesDatabaseContext.Charges
                .Include(x => x.ChargePeriodDetails)
                .Include(x => x.ChargePrices)
                .Include(x => x.MarketParticipant)
                .SingleAsync(x => x.ChargeId == chargeId &&
                                           x.Owner == owner &&
                                           x.ChargeType == (int)chargeType).ConfigureAwait(false);

            return ChargeMapper.MapChargeToChargeDomainModel(charge);
        }

        public async Task<bool> CheckIfChargeExistsAsync(string chargeId, string owner, ChargeType chargeType)
        {
            return await _chargesDatabaseContext.Charges
                .AnyAsync(x => x.ChargeId == chargeId &&
                                        x.Owner == owner &&
                                        x.ChargeType == (int)chargeType).ConfigureAwait(false);
        }

        public async Task<bool> CheckIfChargeExistsByCorrelationIdAsync(string correlationId)
        {
            return await _chargesDatabaseContext.ChargeOperations
                .AnyAsync(x => x.CorrelationId == correlationId)
                .ConfigureAwait(false);
        }

        public async Task StoreChargeAsync(Charge newCharge)
        {
            if (newCharge == null) throw new ArgumentNullException(nameof(newCharge));

            var marketParticipant = await GetMarketParticipantAsync(newCharge.Document.Sender.Id).ConfigureAwait(false);

            var chargeOperation = ChargeMapper.MapToChargeOperation(newCharge, marketParticipant);

            await _chargesDatabaseContext.ChargeOperations.AddAsync(chargeOperation).ConfigureAwait(false);

            await _chargesDatabaseContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task<MarketParticipant> GetMarketParticipantAsync(string marketParticipantId)
        {
            return await _chargesDatabaseContext.MarketParticipants.SingleAsync(x =>
                x.MarketParticipantId == marketParticipantId).ConfigureAwait(false);
        }
    }
}
