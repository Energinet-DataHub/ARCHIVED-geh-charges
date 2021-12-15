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
using GreenEnergyHub.Charges.Domain.AvailableChargeData;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinkReceiptData;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Domain.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.Domain.AvailableData;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.DefaultChargeLinks;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.Context
{
    /// <summary>
    /// Contract defining the capabilities of the Charges database context.
    /// </summary>
    public interface IChargesDatabaseContext
    {
        DbSet<TAvailableData> SetAsync<TAvailableData>()
            where TAvailableData : AvailableDataBase;

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
        /// Charge links available in the database.
        /// </summary>
        DbSet<ChargeLink> ChargeLinks { get; }

        /// <summary>
        /// DefaultChargeLink available in the database.
        /// </summary>
        DbSet<DefaultChargeLink> DefaultChargeLinks { get; }

        /// <summary>
        /// AvailableChargeData available in the database.
        /// </summary>
        DbSet<AvailableChargeData> AvailableChargeData { get; }

        /// <summary>
        /// AvailableChargeReceiptData available in the database
        /// </summary>
        DbSet<AvailableChargeReceiptData> AvailableChargeReceiptData { get; }

        /// <summary>
        /// AvailableChargeLinksData available in the database.
        /// </summary>
        DbSet<AvailableChargeLinksData> AvailableChargeLinksData { get; }

        /// <summary>
        /// AvailableChargeLinkReceiptData available in the database
        /// </summary>
        DbSet<AvailableChargeLinkReceiptData> AvailableChargeLinkReceiptData { get; }

        /// <summary>
        /// Saves changes to the database.
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
