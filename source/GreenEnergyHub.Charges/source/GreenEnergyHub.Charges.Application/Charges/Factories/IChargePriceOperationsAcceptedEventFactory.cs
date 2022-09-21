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
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Application.Charges.Factories
{
    /// <summary>
    /// Factory for creating <see cref="ChargePriceOperationsAcceptedEvent"/>
    /// </summary>
    public interface IChargePriceOperationsAcceptedEventFactory
    {
        /// <summary>
        /// Factory method for creating <see cref="ChargePriceOperationsAcceptedEvent"/> based on accepted operations
        /// </summary>
        /// <param name="document">Document containing information about market participant causing the event</param>
        /// <param name="operations">Operations to be confirmed</param>
        /// <returns><see cref="ChargePriceOperationsAcceptedEvent"/></returns>
        ChargePriceOperationsAcceptedEvent Create(
            DocumentDto document,
            IReadOnlyCollection<ChargePriceOperationDto> operations);
    }
}
