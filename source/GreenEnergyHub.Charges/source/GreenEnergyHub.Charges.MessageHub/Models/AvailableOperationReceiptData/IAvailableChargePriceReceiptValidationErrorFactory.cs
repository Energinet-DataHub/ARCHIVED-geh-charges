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

using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableOperationReceiptData
{
    /// <summary>
    /// Factory for creating <see cref="AvailableReceiptValidationError"/>
    /// </summary>
    public interface IAvailableChargePriceReceiptValidationErrorFactory
    {
        /// <summary>
        /// Factory method for creating <see cref="AvailableReceiptValidationError"/>
        /// </summary>
        /// <param name="validationError"></param>
        /// <param name="document"></param>
        /// <param name="chargePriceOperationDto"></param>
        /// <returns><see cref="AvailableReceiptValidationError"/></returns>
        AvailableReceiptValidationError Create(
            ValidationError validationError,
            DocumentDto document,
            ChargePriceOperationDto chargePriceOperationDto);
    }
}
