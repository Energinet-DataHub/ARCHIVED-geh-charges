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
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.Context
{
    public class ChargesDatabaseContext : DbContext, IChargesDatabaseContext
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public ChargesDatabaseContext(DbContextOptions<ChargesDatabaseContext> options)
            : base(options)
        {
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public DbSet<ChargePrice> ChargePrices { get; set; }

        public DbSet<ChargeOperation> ChargeOperations { get; set; }

        public DbSet<ChargePeriodDetails> ChargePeriodDetails { get; set; }

        public DbSet<Charge> Charges { get; set; }

        public DbSet<MarketParticipant> MarketParticipants { get; set; }

        public DbSet<MeteringPoint> MeteringPoints { get; set; }

        public DbSet<DefaultChargeLink> DefaultChargeLinks { get; set; }

        public Task<int> SaveChangesAsync()
           => base.SaveChangesAsync();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Charges");

            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.Entity<ChargePrice>().ToTable("ChargePrice");
            modelBuilder.Entity<ChargeOperation>().ToTable("ChargeOperation");
            modelBuilder.Entity<ChargePeriodDetails>().ToTable("ChargePeriodDetails");
            modelBuilder.Entity<Charge>().ToTable("Charge");
            modelBuilder.Entity<MarketParticipant>().ToTable("MarketParticipant");
            modelBuilder.Entity<MeteringPoint>().ToTable("MeteringPoint");
            modelBuilder.Entity<DefaultChargeLink>().ToTable("DefaultChargeLinkSetting");

            base.OnModelCreating(modelBuilder);
        }
    }
}
