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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands
{
    public class ChargeCommandFactory : IChargeCommandFactory
    {
        public ChargeCommand CreateFromCharge([NotNull] Charge charge, DocumentDto document)
        {
            var chargeCommand = new ChargeCommand
            {
                Document = document,
                ChargeOperation = new ChargeOperation
                {
                    Id = charge.ChargeOperationId,
                    Type = charge.Type,
                    ChargeId = charge.SenderProvidedChargeId,
                    ChargeName = charge.Name,
                    ChargeDescription = charge.Description,
                    ChargeOwner = charge.Owner,
                    Resolution = charge.Resolution,
                    TaxIndicator = charge.TaxIndicator,
                    TransparentInvoicing = charge.TransparentInvoicing,
                    VatClassification = charge.VatClassification,
                    StartDateTime = charge.StartDateTime,
                    EndDateTime = charge.EndDateTime,
                    Points = charge.Points,
                },
            };

            return chargeCommand;
        }
    }
}
