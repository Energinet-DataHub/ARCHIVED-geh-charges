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

using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands
{
    public class ChargeCommandFactory : IChargeCommandFactory
    {
        public ChargeCommand CreateFromCharge(Charge charge, DocumentDto document)
        {
            var chargeCommand = new ChargeCommand
            {
                Document = document,
                ChargeOperation = new ChargeOperation(
                    charge.ChargeOperationId,
                    charge.Type,
                    charge.SenderProvidedChargeId,
                    charge.Name,
                    charge.Description,
                    charge.Owner,
                    charge.Resolution,
                    charge.TaxIndicator,
                    charge.TransparentInvoicing,
                    charge.VatClassification,
                    charge.StartDateTime,
                    charge.EndDateTime,
                    charge.Points),
            };

            return chargeCommand;
        }
    }
}
