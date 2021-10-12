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

namespace GreenEnergyHub.Charges.Domain.MeteringPoints
{
    /// <summary>
    /// Repository for Metering Points.
    /// </summary>
    public interface IMeteringPointRepository
    {
        /// <summary>
        /// Saves the supplied metering point to the database.
        /// </summary>
        /// <param name="meteringPoint"></param>
        Task StoreMeteringPointAsync(MeteringPoint meteringPoint);

        /// <summary>
        /// Used to find a Metering Point.
        /// </summary>
        /// <param name="meteringPointId"></param>
        /// <returns>Metering Point</returns>
        Task<MeteringPoint> GetMeteringPointAsync(string meteringPointId);

        /// <summary>
        /// Used to find a Metering Point by Id.
        /// </summary>
        /// <returns>Metering Point</returns>
        Task<MeteringPoint> GetMeteringPointAsync(Guid id);

        /// <summary>
        /// Check if the metering point id is already stored
        /// </summary>
        /// <param name="meteringPointId"></param>
        /// <returns>bool</returns>
        Task<bool> CheckIfMeteringPointExistsAsync(string meteringPointId);
    }
}
