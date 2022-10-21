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
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.DefaultChargeLinks;
using GreenEnergyHub.Charges.Domain.GridAreaLinks;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.Persistence
{
    /// <summary>
    /// Context representing the Charges database
    /// </summary>
    public interface IChargesDatabaseContext
    {
        /// <summary>
        /// Charges
        /// </summary>
        DbSet<Charge> Charges { get; }

        /// <summary>
        /// ChargeMessages
        /// </summary>
        DbSet<ChargeMessage> ChargeMessages { get; }

        /// <summary>
        /// MarketParticipants
        /// </summary>
        DbSet<MarketParticipant> MarketParticipants { get; }

        /// <summary>
        /// MeteringPoints
        /// </summary>
        DbSet<MeteringPoint> MeteringPoints { get; }

        /// <summary>
        /// GridAreaLinks
        /// </summary>
        DbSet<GridAreaLink> GridAreaLinks { get; }

        /// <summary>
        /// ChargesLinks
        /// </summary>
        DbSet<ChargeLink> ChargeLinks { get; }

        /// <summary>
        /// DefaultChargeLinks
        /// </summary>
        DbSet<DefaultChargeLink> DefaultChargeLinks { get; }

        /// <summary>
        /// OutboxMessages
        /// </summary>
        DbSet<OutboxMessage> OutboxMessages { get; }

        /// <summary>
        /// Saves changes to the database.
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
