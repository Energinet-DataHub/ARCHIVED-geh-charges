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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChargeCommands;

namespace GreenEnergyHub.Charges.Domain.Charges
{
    public class ChargeFactory : IChargeFactory
    {
        public Task<Charge> CreateFromCommandAsync([NotNull]ChargeCommand command)
        {
            var c = new Charge(
                Guid.NewGuid(),
                command.Document,
                command.ChargeOperation.Id,
                command.ChargeOperation.ChargeId,
                command.ChargeOperation.ChargeName,
                command.ChargeOperation.ChargeDescription,
                command.ChargeOperation.ChargeOwner,
                command.CorrelationId,
                command.ChargeOperation.StartDateTime,
                command.ChargeOperation.EndDateTime,
                command.ChargeOperation.Type,
                command.ChargeOperation.VatClassification,
                command.ChargeOperation.Resolution,
                command.ChargeOperation.TransparentInvoicing,
                command.ChargeOperation.TaxIndicator,
                command.ChargeOperation.Points);

            return Task.FromResult(c);
        }
    }
}
