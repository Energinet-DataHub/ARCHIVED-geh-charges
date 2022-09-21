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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Domain.MarketParticipants
{
    /// <summary>
    /// Repository for managing market participants.
    /// </summary>
    public interface IMarketParticipantRepository
    {
        /// <summary>
        /// Adds a new market participant
        /// </summary>
        /// <param name="marketParticipant"></param>
        Task AddAsync(MarketParticipant marketParticipant);

        /// <summary>
        /// Get single actor by ActorId or null
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<MarketParticipant?> GetByActorIdAsync(Guid? actorId);

        /// <summary>
        /// Get single actor or null from b2CActorId
        /// </summary>
        /// <param name="b2CActorId"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<MarketParticipant?> SingleOrNullAsync(Guid? b2CActorId);

        /// <summary>
        /// Get single actor or null from gln/eic no
        /// </summary>
        /// <param name="marketParticipantId"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<MarketParticipant?> SingleOrNullAsync(string marketParticipantId);

        /// <summary>
        /// Retrieves a market participant from role and gln/eic no.
        /// </summary>
        Task<MarketParticipant?> SingleOrNullAsync(
            MarketParticipantRole businessProcessRole,
            string marketParticipantId);

        Task<IReadOnlyCollection<MarketParticipant>> GetAsync(IEnumerable<Guid> ids);

        /// <summary>
        /// Get all the active grid energy suppliers
        /// </summary>
        Task<List<MarketParticipant>> GetActiveEnergySuppliersAsync();

        /// <summary>
        /// Get all the active grid access providers
        /// </summary>
        Task<List<MarketParticipant>> GetGridAccessProvidersAsync();

        /// <summary>
        /// Get the grid access provider from primary key
        /// </summary>
        Task<MarketParticipant?> GetGridAccessProviderAsync(Guid gridAreaId);

        /// <summary>
        /// Get the grid access provider of the grid area that the metering point belongs to.
        /// </summary>
        Task<MarketParticipant> GetGridAccessProviderAsync(string meteringPointId);

        Task<MarketParticipant> GetMeteringPointAdministratorAsync();

        Task<MarketParticipant> GetSystemOperatorAsync();

        /// <summary>
        /// Retrieves an active market participant from gln/eic no. with the role EZ or DDM
        /// </summary>
        Task<MarketParticipant> GetSystemOperatorOrGridAccessProviderAsync(string marketParticipantId);
    }
}
