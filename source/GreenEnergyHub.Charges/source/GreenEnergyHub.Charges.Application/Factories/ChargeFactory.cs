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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.Factories
{
    public class ChargeFactory : IChargeFactory
    {
        public Task<Charge> CreateFromCommandAsync([NotNull]ChargeCommand command)
        {
            var c = new Charge
            {
                Document = command.Document,
                Id = command.ChargeOperation.ChargeId,
                Description = command.ChargeOperation.ChargeDescription,
                Name = command.ChargeOperation.ChargeName,
                Owner = command.ChargeOperation.ChargeOwner,
                Points = command.ChargeOperation.Points,
                Resolution = command.ChargeOperation.Resolution,
                TaxIndicator = command.ChargeOperation.TaxIndicator,
                Type = command.ChargeOperation.Type,
                VatClassification = command.ChargeOperation.VatClassification,
                TransparentInvoicing = command.ChargeOperation.TransparentInvoicing,
                EndDateTime = command.ChargeOperation.EndDateTime,
                StartDateTime = command.ChargeOperation.StartDateTime,
                Status = command.ChargeOperation.OperationType,
                ChargeOperationId = command.ChargeOperation.Id,
                LastUpdatedBy = "Volt", // This should be used to identify the user.
            };
            // Right now CorrelationId is not a part of the Charge, but its needed for persistence.
            c.CorrelationId = command.CorrelationId;
            return Task.FromResult(c);
        }
    }
}
