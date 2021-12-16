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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Context.EntityConfigurations;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinkReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Context
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "Private setters are needed by EF Core")]
    public class MessageHubDatabaseContext : DbContext, IMessageHubDatabaseContext
    {
        #nullable disable
        public MessageHubDatabaseContext(DbContextOptions<MessageHubDatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<AvailableChargeData> AvailableChargeData { get; private set; }

        public DbSet<AvailableChargeReceiptData> AvailableChargeReceiptData { get; private set; }

        public DbSet<AvailableChargeLinksData> AvailableChargeLinksData { get; private set; }

        public DbSet<AvailableChargeLinkReceiptData> AvailableChargeLinkReceiptData { get; private set; }

        public Task<int> SaveChangesAsync()
           => base.SaveChangesAsync();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(DatabaseSchemaNames.MessageHub);

            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.ApplyConfiguration(new AvailableChargeDataConfiguration());
            modelBuilder.ApplyConfiguration(new AvailableChargeReceiptDataConfiguration());
            modelBuilder.ApplyConfiguration(new AvailableChargeLinksDataConfiguration());
            modelBuilder.ApplyConfiguration(new AvailableChargeLinkReceiptDataConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
