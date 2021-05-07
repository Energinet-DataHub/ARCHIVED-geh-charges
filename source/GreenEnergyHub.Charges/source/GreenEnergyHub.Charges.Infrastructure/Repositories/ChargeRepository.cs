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
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using GreenEnergyHub.Charges.Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;
using Charge = GreenEnergyHub.Charges.Domain.Charge;
using ChargeType = GreenEnergyHub.Charges.Infrastructure.Context.Model.ChargeType;
using MarketParticipant = GreenEnergyHub.Charges.Infrastructure.Context.Model.MarketParticipant;

namespace GreenEnergyHub.Charges.Infrastructure.Repositories
{
    public class ChargeRepository : IChargeRepository
    {
        private readonly IChargesDatabaseContext _chargesDatabaseContext;

        public ChargeRepository(IChargesDatabaseContext chargesDatabaseContext)
        {
            _chargesDatabaseContext = chargesDatabaseContext;
        }

        public async Task<Charge> GetChargeAsync(string mrid, string chargeTypeOwnerMRid)
        {
            var charge = await _chargesDatabaseContext.Charge
                .Include(x => x.ChargeTypeOwner)
                .SingleAsync(x => x.MRid == mrid &&
                                           x.ChargeTypeOwner.MRid == chargeTypeOwnerMRid).ConfigureAwait(false);

            return ChangeOfChargesMapper.MapChargeToChangeOfChargesMessage(charge);
        }

        public async Task<bool> CheckIfChargeExistsAsync(string mrid, string chargeTypeOwnerMRid)
        {
            return await _chargesDatabaseContext.Charge
                .Include(x => x.ChargeTypeOwner)
                .AnyAsync(x => x.MRid == mrid &&
                                        x.ChargeTypeOwner.MRid == chargeTypeOwnerMRid).ConfigureAwait(false);
        }

        public async Task StoreChargeAsync(Charge newCharge)
        {
            if (newCharge == null) throw new ArgumentNullException(nameof(newCharge));

            var chargeType = await GetChargeTypeAsync(newCharge).ConfigureAwait(false);
            if (chargeType == null) throw new Exception($"No charge type for {newCharge.Type}");

            var resolutionType = await GetResolutionTypeAsync(newCharge).ConfigureAwait(false);
            if (resolutionType == null) throw new Exception($"No resolution type for {newCharge.Resolution}");

            var vatPayerType = await GetVatPayerTypeAsync(newCharge).ConfigureAwait(false);
            if (vatPayerType == null) throw new Exception($"No VAT payer type for {newCharge.VatClassification}");

            var chargeTypeOwnerMRid = await GetChargeTypeOwnerMRidAsync(newCharge).ConfigureAwait(false);
            if (chargeTypeOwnerMRid == null) throw new Exception($"No market participant for {newCharge.Owner}");

            var charge = ChangeOfChargesMapper.MapDomainChargeToCharge(
                newCharge,
                chargeType,
                chargeTypeOwnerMRid,
                resolutionType,
                vatPayerType);

            await _chargesDatabaseContext.Charge.AddAsync(charge).ConfigureAwait(false);
            await _chargesDatabaseContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task<MarketParticipant?> GetChargeTypeOwnerMRidAsync(Charge chargeMessage)
        {
            return string.IsNullOrWhiteSpace(chargeMessage.Owner)
                ? throw new ArgumentException($"Fails as {nameof(chargeMessage.Owner)} is invalid")
                : await _chargesDatabaseContext.MarketParticipant.SingleOrDefaultAsync(type =>
                type.MRid == chargeMessage.Owner).ConfigureAwait(false);
        }

        private async Task<VatPayerType?> GetVatPayerTypeAsync(Charge chargeMessage)
        {
            // If we start using a enum for Vat does it make sense to check if it exists?
            return string.IsNullOrWhiteSpace(chargeMessage.VatClassification.ToString())
                ? throw new ArgumentException($"Fails as {nameof(chargeMessage.VatClassification)} is invalid")
                // Right now we cant support vat not being of type D01 or D02, After we refactor or DB scheme it will be update.
                : await _chargesDatabaseContext.VatPayerType.SingleOrDefaultAsync(type =>
                    type.Name == "D01").ConfigureAwait(false);
        }

        private async Task<ResolutionType?> GetResolutionTypeAsync(Charge chargeMessage)
        {
            // If we start using a enum for Resolution does it make sense to check if it exists?
            return string.IsNullOrWhiteSpace(chargeMessage.Resolution.ToString())
                ? throw new ArgumentException($"Fails as {nameof(chargeMessage.Resolution)} is invalid")
                : await _chargesDatabaseContext.ResolutionType.SingleOrDefaultAsync(type => type.Name == chargeMessage.Resolution.ToString()).ConfigureAwait(false);
        }

        private async Task<ChargeType?> GetChargeTypeAsync(Charge chargeMessage)
        {
            // If we start using a enum for Chargetype does it make sense to check if it exists?
            return string.IsNullOrWhiteSpace(chargeMessage.Type.ToString())
                ? throw new ArgumentException($"Fails as {nameof(chargeMessage.Type)} is invalid")
                : await _chargesDatabaseContext.ChargeType.SingleOrDefaultAsync(type => type.Name == chargeMessage.Type.ToString()).ConfigureAwait(false);
        }
    }
}
