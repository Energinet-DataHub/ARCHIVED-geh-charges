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

using System.Threading.Tasks;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.Context
{
    /// <summary>
    /// Contract defining the capabilities of the Charges database context.
    /// </summary>
    public interface IChargesDatabaseContext
    {
        /// <summary>
        /// ChargePrices available in the database.
        /// </summary>
        DbSet<ChargePrice> ChargePrices { get; }

        /// <summary>
        /// ChargeOperations available in the database.
        /// </summary>
        DbSet<ChargeOperation> ChargeOperations { get; }

        /// <summary>
        /// ChargePeriodDetails available in the database.
        /// </summary>
        DbSet<ChargePeriodDetails> ChargePeriodDetails { get; }

        /// <summary>
        /// Charges available in the database.
        /// </summary>
        DbSet<Charge> Charges { get; }

        /// <summary>
        /// MarketParticipants available in the database.
        /// </summary>
        DbSet<MarketParticipant> MarketParticipants { get; }

        /// <summary>
        /// Metering Point available in the database.
        /// </summary>
        DbSet<MeteringPoint> MeteringPoints { get; }

        /// <summary>
        /// DefaultChargeLinkSetting available in the database.
        /// </summary>
        DbSet<DefaultChargeLinkSetting> DefaultChargeLinkSettings { get; }

        /// <summary>
        /// Saves changes to the database.
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
