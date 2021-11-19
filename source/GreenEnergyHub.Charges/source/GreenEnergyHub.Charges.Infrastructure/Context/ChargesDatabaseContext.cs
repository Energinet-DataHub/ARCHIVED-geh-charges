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
using GreenEnergyHub.Charges.Domain.AvailableChargeData;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinkReceiptData;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Context.EntityConfigurations;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.Context
{
    public class ChargesDatabaseContext : DbContext, IChargesDatabaseContext
    {
        #nullable disable
        public ChargesDatabaseContext(DbContextOptions<ChargesDatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<ChargePrice> ChargePrices { get; private set; }

        public DbSet<ChargeOperation> ChargeOperations { get; private set; }

        public DbSet<ChargePeriodDetails> ChargePeriodDetails { get; private set; }

        public DbSet<Charge> Charges { get; private set; }

        public DbSet<MarketParticipant> MarketParticipants { get; private set; }

        public DbSet<MeteringPoint> MeteringPoints { get; private set; }

        public DbSet<DefaultChargeLink> DefaultChargeLinks { get; private set; }

        public DbSet<ChargeLink> ChargeLinks { get; private set; }

        public DbSet<AvailableChargeLinksData> AvailableChargeLinksData { get; private set; }

        public DbSet<AvailableChargeData> AvailableChargeData { get; private set; }

        public DbSet<AvailableChargeLinkReceiptData> AvailableChargeLinkReceiptData { get; private set; }

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
            modelBuilder.Entity<DefaultChargeLink>().ToTable("DefaultChargeLink");

            modelBuilder.ApplyConfiguration(new ChargeLinkEntityConfiguration());
            modelBuilder.ApplyConfiguration(new MeteringPointEntityConfiguration());
            modelBuilder.ApplyConfiguration(new AvailableChargeLinksDataConfiguration());
            modelBuilder.ApplyConfiguration(new AvailableChargeDataConfiguration());
            modelBuilder.ApplyConfiguration(new AvailableChargeLinkReceiptDataConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
