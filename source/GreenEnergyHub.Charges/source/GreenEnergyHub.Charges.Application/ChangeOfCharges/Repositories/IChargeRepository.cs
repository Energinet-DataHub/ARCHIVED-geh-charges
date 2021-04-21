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
using GreenEnergyHub.Charges.Domain;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.ChangeOfCharges.Repositories
{
    /// <summary>
    /// Contract defining the capabilities of the infrastructure component facilitating interaction with the charges data store.
    /// </summary>
    public interface IChargeRepository
    {
        /// <summary>
        /// Stores the given <see cref="ChargeCommand"/> in persistent storage.
        /// </summary>
        /// <param name="command">The command to be persisted.</param>
        /// <returns>A <see cref="ChargeStorageStatus"/> to indicate if the operation was performed successfully.</returns>
        Task<ChargeStorageStatus> StoreChargeAsync(ChargeCommand command);

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns>TODO 2</returns>
        Task<Charge> GetChargeAsync();
    }
}
