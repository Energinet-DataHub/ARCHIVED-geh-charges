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

using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;

namespace GreenEnergyHub.Charges.Domain.Charges
{
    /// <summary>
    /// Factory to create ChargeIdentifier
    /// </summary>
    public interface IChargeIdentifierFactory
    {
        /// <summary>
        /// Factory method to create ChargeIdentifier from <see cref="ChargeInformationDto"/>
        /// </summary>
        /// <param name="chargeId"></param>
        /// <param name="chargeType"></param>
        /// <param name="chargeOwner"></param>
        /// <returns>A <see cref="Task{ChargeIdentifier}"/> representing the result of the asynchronous operation.</returns>
        Task<ChargeIdentifier> CreateAsync(string chargeId, ChargeType chargeType, string chargeOwner);
    }
}
