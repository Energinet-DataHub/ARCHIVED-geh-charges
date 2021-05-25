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
using GreenEnergyHub.Charges.Infrastructure.Context;
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

        public async Task<Charge> GetChargeAsync(string chargeId, string owner)
        {
            var charge = await _chargesDatabaseContext.Charge
                .Include(x => x.ChargePeriodDetails)
                .Include(x => x.ChargePrices)
                .SingleAsync(x => x.ChargeId == chargeId &&
                                           x.Owner == owner).ConfigureAwait(false);

            return ChargeMapper.MapChargeToChangeOfChargesMessage(charge);
        }

        public async Task<bool> CheckIfChargeExistsAsync(string chargeId, string owner)
        {
            return await _chargesDatabaseContext.Charge
                .AnyAsync(x => x.ChargeId == chargeId &&
                                        x.Owner == owner).ConfigureAwait(false);
        }

        public async Task<bool> CheckIfChargeExistsByCorrelationIdAsync(string correlationId)
        {
            return await _chargesDatabaseContext.ChargeOperation
                .AnyAsync(x => x.CorrelationId == correlationId)
                .ConfigureAwait(false);
        }

        public async Task StoreChargeAsync(Charge newCharge)
        {
            if (newCharge == null) throw new ArgumentNullException(nameof(newCharge));

            // Charge.
            var charge = ChargeMapper.MapDomainChargeToCharge(newCharge);

            var createdCharge = await _chargesDatabaseContext.Charge.AddAsync(charge).ConfigureAwait(false);

            await _chargesDatabaseContext.SaveChangesAsync().ConfigureAwait(false);

            // ChargeOperation.
            var chargeOperation = ChargeMapper.CreateChargeOperation(newCharge, createdCharge.Entity.RowId);

            var createdChargeOperation = await _chargesDatabaseContext.ChargeOperation.AddAsync(chargeOperation).ConfigureAwait(false);

            await _chargesDatabaseContext.SaveChangesAsync().ConfigureAwait(false);

            // ChargeDetails.
            var chargeDetails =
                ChargeMapper.CreateChargePeriodDetails(newCharge, createdCharge.Entity.RowId, createdChargeOperation.Entity.RowId);

            await _chargesDatabaseContext.ChargePeriodDetails.AddAsync(chargeDetails).ConfigureAwait(false);

            await _chargesDatabaseContext.SaveChangesAsync().ConfigureAwait(false);

            // ChargePrices
            var chargePrices = ChargeMapper.CreateChargePrice(
                newCharge,
                createdCharge.Entity.RowId,
                createdChargeOperation.Entity.RowId);

            foreach (var chargePrice in chargePrices)
            {
                await _chargesDatabaseContext.ChargePrice.AddAsync(chargePrice).ConfigureAwait(false);
            }

            await _chargesDatabaseContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
