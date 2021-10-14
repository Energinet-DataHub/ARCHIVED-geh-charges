﻿// Copyright 2020 Energinet DataHub A/S
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Context.Mapping;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using Microsoft.EntityFrameworkCore;
using Charge = GreenEnergyHub.Charges.Domain.Charges.Charge;

namespace GreenEnergyHub.Charges.Infrastructure.Repositories
{
    public class ChargeRepository : IChargeRepository
    {
        private readonly IChargesDatabaseContext _chargesDatabaseContext;

        public ChargeRepository(IChargesDatabaseContext chargesDatabaseContext)
        {
            _chargesDatabaseContext = chargesDatabaseContext;
        }

        public async Task<Charge> GetChargeAsync(ChargeSenderIdentifier chargeSenderIdentifier)
        {
            var charge = await GetChargesAsQueryable()
                .SingleAsync(x => x.SenderProvidedChargeId == chargeSenderIdentifier.senderProvidedChargeId &&
                                           x.MarketParticipant.MarketParticipantId == chargeSenderIdentifier.owner &&
                                           x.ChargeType == (int)chargeSenderIdentifier.chargeType)
                .ConfigureAwait(false);

            return ChargeMapper.MapChargeToChargeDomainModel(charge);
        }

        public async Task<IReadOnlyCollection<Charge>> GetChargesAsync(
            IReadOnlyCollection<ChargeSenderIdentifier> chargeSenderIdentifiers)
        {
            var ccc = await GetChargesAsQueryable().Where(charge =>
                chargeSenderIdentifiers.Any(chargeSenderIdentifier =>
                (int)chargeSenderIdentifier.chargeType == charge.ChargeType &&
                chargeSenderIdentifier.senderProvidedChargeId == charge.SenderProvidedChargeId &&
                chargeSenderIdentifier.owner == charge.MarketParticipant.MarketParticipantId)).ToListAsync().ConfigureAwait(false);

            return ccc.Select(ChargeMapper.MapChargeToChargeDomainModel).ToList();
        }

        public async Task<Charge> GetChargeAsync(Guid id)
        {
            var charge = await GetChargesAsQueryable()
                .SingleAsync(x => x.Id == id)
                .ConfigureAwait(false);

            return ChargeMapper.MapChargeToChargeDomainModel(charge);
        }

        public async Task<bool> CheckIfChargeExistsAsync(ChargeSenderIdentifier chargeSenderIdentifier)
        {
            return await _chargesDatabaseContext.Charges
                .AnyAsync(x => x.SenderProvidedChargeId == chargeSenderIdentifier.senderProvidedChargeId &&
                                        x.MarketParticipant.MarketParticipantId == chargeSenderIdentifier.owner &&
                                        x.ChargeType == (int)chargeSenderIdentifier.chargeType)
                .ConfigureAwait(false);
        }

        public async Task StoreChargeAsync(Charge newCharge)
        {
            if (newCharge == null) throw new ArgumentNullException(nameof(newCharge));
            var marketParticipant = await GetMarketParticipantAsync(newCharge.Document.Sender.Id).ConfigureAwait(false);
            var charge = ChargeMapper.MapDomainChargeToCharge(newCharge, marketParticipant);
            await _chargesDatabaseContext.Charges.AddAsync(charge).ConfigureAwait(false);
            await _chargesDatabaseContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private IQueryable<Context.Model.Charge> GetChargesAsQueryable()
        {
            return _chargesDatabaseContext.Charges
                .Include(x => x.ChargePeriodDetails)
                .Include(x => x.ChargePrices)
                .Include(x => x.MarketParticipant)
                .Include(x => x.ChargeOperation)
                .AsQueryable();
        }

        private async Task<MarketParticipant> GetMarketParticipantAsync(string marketParticipantId)
        {
            return await _chargesDatabaseContext.MarketParticipants
                .SingleAsync(x => x.MarketParticipantId == marketParticipantId)
                .ConfigureAwait(false);
        }
    }
}
