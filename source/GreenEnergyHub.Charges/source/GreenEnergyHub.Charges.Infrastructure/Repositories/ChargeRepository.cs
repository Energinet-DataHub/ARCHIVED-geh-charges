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

        public async Task<ChargeStorageStatus> StoreChargeAsync(Domain.Charge command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var chargeType = await GetChargeTypeAsync(command).ConfigureAwait(false);
            if (chargeType == null) return ChargeStorageStatus.CreateFailure($"No charge type for {command.Type}");

            var resolutionType = await GetResolutionTypeAsync(command).ConfigureAwait(false);
            if (resolutionType == null) return ChargeStorageStatus.CreateFailure($"No resolution type for {command.Period?.Resolution}");

            var vatPayerType = await GetVatPayerTypeAsync(command).ConfigureAwait(false);
            if (vatPayerType == null) return ChargeStorageStatus.CreateFailure($"No VAT payer type for {command.MktActivityRecord?.ChargeType?.VatPayer}");

            var chargeTypeOwnerMRid = await GetChargeTypeOwnerMRidAsync(command).ConfigureAwait(false);
            if (chargeTypeOwnerMRid == null) return ChargeStorageStatus.CreateFailure($"No market participant for {command.ChargeTypeOwnerMRid}");

            var charge = ChangeOfChargesMapper.MapChangeOfChargesTransactionToCharge(command, chargeType, chargeTypeOwnerMRid, resolutionType, vatPayerType);

            await _chargesDatabaseContext.Charge.AddAsync(charge).ConfigureAwait(false);
            await _chargesDatabaseContext.SaveChangesAsync().ConfigureAwait(false);
            return ChargeStorageStatus.CreateSuccess();
        }

        public Task<Charge> GetChargeAsync()
        {
            throw new NotImplementedException();
        }

        private async Task<MarketParticipant?> GetChargeTypeOwnerMRidAsync(ChargeCommand chargeMessage)
        {
            return string.IsNullOrWhiteSpace(chargeMessage.ChargeTypeOwnerMRid)
                ? throw new ArgumentException($"Fails as {nameof(chargeMessage.ChargeTypeOwnerMRid)} is invalid")
                : await _chargesDatabaseContext.MarketParticipant.SingleOrDefaultAsync(type =>
                type.MRid == chargeMessage.ChargeTypeOwnerMRid).ConfigureAwait(false);
        }

        private async Task<VatPayerType?> GetVatPayerTypeAsync(ChargeCommand chargeMessage)
        {
            return string.IsNullOrWhiteSpace(chargeMessage.MktActivityRecord?.ChargeType?.VatPayer)
                ? throw new ArgumentException($"Fails as {nameof(chargeMessage.MktActivityRecord.ChargeType.VatPayer)} is invalid")
                : await _chargesDatabaseContext.VatPayerType.SingleOrDefaultAsync(type =>
                type.Name == chargeMessage.MktActivityRecord.ChargeType.VatPayer).ConfigureAwait(false);
        }

        private async Task<ResolutionType?> GetResolutionTypeAsync(ChargeCommand chargeMessage)
        {
            return string.IsNullOrWhiteSpace(chargeMessage.Period?.Resolution)
                ? throw new ArgumentException($"Fails as {nameof(chargeMessage.Period.Resolution)} is invalid")
                : await _chargesDatabaseContext.ResolutionType.SingleOrDefaultAsync(type => type.Name == chargeMessage.Period.Resolution).ConfigureAwait(false);
        }

        private async Task<ChargeType?> GetChargeTypeAsync(ChargeCommand chargeMessage)
        {
            return string.IsNullOrWhiteSpace(chargeMessage.Type)
                ? throw new ArgumentException($"Fails as {nameof(chargeMessage.Type)} is invalid")
                : await _chargesDatabaseContext.ChargeType.SingleOrDefaultAsync(type => type.Code == chargeMessage.Type).ConfigureAwait(false);
        }
    }
}
