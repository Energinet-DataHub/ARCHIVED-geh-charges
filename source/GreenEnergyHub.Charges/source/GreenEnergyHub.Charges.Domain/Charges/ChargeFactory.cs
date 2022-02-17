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
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Domain.Charges
{
    public class ChargeFactory : IChargeFactory
    {
        private readonly IMarketParticipantRepository _marketParticipantRepository;

        public ChargeFactory(IMarketParticipantRepository marketParticipantRepository)
        {
            _marketParticipantRepository = marketParticipantRepository;
        }

        public async Task<Charge> CreateFromCommandAsync(ChargeCommand command)
        {
            var owner = await _marketParticipantRepository
                .GetOrNullAsync(command.ChargeOperation.ChargeOwner)
                .ConfigureAwait(false);

            if (owner == null)
                throw new InvalidOperationException($"Market participant '{command.ChargeOperation.ChargeOwner}' does not exist.");

            var period = new ChargePeriod(
                Guid.NewGuid(),
                command.ChargeOperation.ChargeName,
                command.ChargeOperation.ChargeDescription,
                command.ChargeOperation.VatClassification,
                command.ChargeOperation.TransparentInvoicing,
                command.ChargeOperation.StartDateTime,
                command.ChargeOperation.EndDateTime.TimeOrEndDefault());

            return new Charge(
                Guid.NewGuid(),
                command.ChargeOperation.ChargeId,
                owner.Id,
                command.ChargeOperation.Type,
                command.ChargeOperation.Resolution,
                command.ChargeOperation.TaxIndicator,
                command.ChargeOperation.Points,
                new List<ChargePeriod> { period });
        }
    }
}
