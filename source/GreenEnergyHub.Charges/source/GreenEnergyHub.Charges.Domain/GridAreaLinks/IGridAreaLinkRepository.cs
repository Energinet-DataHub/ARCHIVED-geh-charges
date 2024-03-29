﻿// Copyright 2020 Energinet DataHub A/S
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
using System.Threading.Tasks;

namespace GreenEnergyHub.Charges.Domain.GridAreaLinks
{
    /// <summary>
    /// Repository for managing grid area links.
    /// </summary>
    public interface IGridAreaLinkRepository
    {
        /// <summary>
        /// Persist a new grid area link
        /// </summary>
        /// <param name="gridAreaLink"></param>
        Task AddAsync(GridAreaLink gridAreaLink);

        /// <summary>
        /// Retrieves a grid area link
        /// </summary>
        /// <param name="gridAreaLinkId"></param>
        Task<GridAreaLink?> GetOrNullAsync(Guid gridAreaLinkId);

        /// <summary>
        /// Retrieves a grid area link from grid area
        /// </summary>
        /// <param name="gridAreaId"></param>
        Task<GridAreaLink?> GetGridAreaOrNullAsync(Guid gridAreaId);
    }
}
