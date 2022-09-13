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

using GreenEnergyHub.Charges.Infrastructure.Core.Persistence;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Persistence.EntityConfigurations
{
    public class AvailableChargePriceDataConfiguration : IEntityTypeConfiguration<AvailableChargePriceData>
    {
        public void Configure(EntityTypeBuilder<AvailableChargePriceData> builder)
        {
            builder.ToTable(nameof(AvailableChargePriceData), DatabaseSchemaNames.MessageHub);

            builder.HasKey(x => x.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.Property(x => x.SenderId);
            builder.Property(x => x.SenderRole);
            builder.Property(x => x.RecipientId);
            builder.Property(x => x.RecipientRole);
            builder.Property(x => x.BusinessReasonCode);
            builder.Property(x => x.ChargeId);
            builder.Property(x => x.ChargeOwner);
            builder.Property(x => x.ChargeType);
            builder.Property(x => x.StartDateTime);
            builder.Property(x => x.EndDateTime);
            builder.Property(x => x.Resolution);
            builder.Property(x => x.RequestDateTime);
            builder.Property(x => x.AvailableDataReferenceId);
            builder.Property(x => x.DocumentType);
            builder.Property(x => x.OperationOrder);
            builder.Property(x => x.ActorId);
            builder.Ignore(c => c.Points);
            builder.OwnsMany<AvailableChargePriceDataPoint>("_points", ConfigurePoints);
        }

        private static void ConfigurePoints(OwnedNavigationBuilder<AvailableChargePriceData, AvailableChargePriceDataPoint> points)
        {
            points.WithOwner().HasForeignKey("AvailableChargePriceDataId");
            points.ToTable("AvailableChargePriceDataPoints", DatabaseSchemaNames.MessageHub);
            points.HasKey(p => p.Id);

            points.Property(p => p.Id).ValueGeneratedNever();
            points.Property(d => d.Position);
            points.Property(d => d.Price)
                .HasPrecision(DecimalPrecisionConstants.Precision, DecimalPrecisionConstants.Scale);
        }
    }
}