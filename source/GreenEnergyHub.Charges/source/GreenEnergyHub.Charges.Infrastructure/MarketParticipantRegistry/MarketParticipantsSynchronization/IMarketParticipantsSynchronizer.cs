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
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry.MarketParticipantsSynchronization
{
    /// <summary>
    /// Service to synchronize market participants from the shared market participant registry to the charges domain.
    /// </summary>
    public interface IMarketParticipantsSynchronizer
    {
        /// <summary>
        /// Synchronize market participants from the shared market participant registry to the charges domain.
        /// Market participants can and must only use one specific role each in the charges domain.
        /// Thus only this single role is synchronized and the market participant model only exposes
        /// a single role in <see cref="MarketParticipant.BusinessProcessRole"/>.
        ///
        /// NOTE:
        /// - Business side effects of deactivation are not currently supported.
        /// - Deletion of actors should not occur according to the business. So no market participants
        ///   are ever deleted by this method.
        /// </summary>
        Task SynchronizeAsync();
    }
}
