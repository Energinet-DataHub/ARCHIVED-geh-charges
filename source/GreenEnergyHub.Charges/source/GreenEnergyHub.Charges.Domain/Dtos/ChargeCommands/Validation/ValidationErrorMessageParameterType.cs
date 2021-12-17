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

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation
{
    public enum ValidationErrorMessageParameterType
    {
        ChargeType = 1,
        ChargeTypeOwner = 2,
        Description = 3,
        DocumentType = 4,
        EnergyBusinessProcess = 5,
        EnergyPrice = 6,
        FunctionCode = 7,
        LongDescriptionMaxLength100 = 8,
        MaxOfPosition = 9,
        MessageId = 10,
        MeteringPointId = 11,
        Occurrence = 12,
        PartyChargeTypeId = 13,
        Position = 14,
        ResolutionDuration = 15,
        SenderId = 16,
        TaxIndicator = 17,
        VatClass = 18,
    }
}
