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
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.Context
{
    public class ChargeLinkDatabaseContext : DbContext, IChargeLinkDatabaseContext
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public ChargeLinkDatabaseContext(DbContextOptions<ChargeLinkDatabaseContext> options)
            : base(options)
        {
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public DbSet<ChargeLink> ChargeLinks { get; }

        public Task SaveChangesAsync() => base.SaveChangesAsync();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Charges");

            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.Entity<ChargeLink>(builder =>
            {
                builder.ToTable("ChargeLink");
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
