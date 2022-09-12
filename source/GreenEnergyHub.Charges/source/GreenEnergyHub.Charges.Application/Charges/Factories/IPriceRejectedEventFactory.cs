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

using System.Collections.Generic;
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Application.Charges.Factories
{
    /// <summary>
    /// Factory for creating <see cref="PriceRejectedEvent"/>
    /// </summary>
    public interface IPriceRejectedEventFactory
    {
        /// <summary>
        /// Factory method for creating <see cref="PriceRejectedEvent"/> based on rejected operations and
        /// corresponding validation results.
        /// </summary>
        /// <param name="document">Document containing information about market participant causing the event</param>
        /// <param name="operations">Operations to be rejected</param>
        /// <param name="validationResult">Validation results containing reasons for rejection</param>
        /// <returns><see cref="PriceRejectedEvent"/></returns>
        PriceRejectedEvent Create(
            DocumentDto document,
            IReadOnlyCollection<ChargePriceOperationDto> operations,
            ValidationResult validationResult);
    }
}
