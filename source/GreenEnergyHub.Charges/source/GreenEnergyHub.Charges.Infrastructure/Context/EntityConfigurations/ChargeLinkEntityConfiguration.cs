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
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.Context.EntityConfigurations
{
    public class ChargeLinkEntityConfiguration : IEntityTypeConfiguration<ChargeLink>
    {
        public void Configure(EntityTypeBuilder<ChargeLink> builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.ToTable("ChargeLink");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.ChargeId).HasColumnName("ChargeId");
            builder.Property(c => c.MeteringPointId).HasColumnName("MeteringPointId");

            builder.OwnsMany<ChargeLinkOperation>("_operations", ConfigureOperations);
            builder.OwnsMany<ChargeLinkPeriodDetails>("_periodDetails", ConfigurePeriodDetails);
        }

        private static void ConfigurePeriodDetails(OwnedNavigationBuilder<ChargeLink, ChargeLinkPeriodDetails> details)
        {
            details.WithOwner().HasForeignKey("ChargeLinkId");

            details.ToTable("ChargeLinkPeriodDetails");

            details.HasKey(p => p.Id);

            details.Property(p => p.Id).ValueGeneratedNever();
            details.Property(d => d.Factor).HasColumnName("Factor");
            details.Property(d => d.CreatedByOperationId).HasColumnName("CreatedByOperationId");
            details.Property(d => d.RetiredByOperationId).HasColumnName("RetiredByOperationId");

            details.Property(d => d.StartDateTime)
                .HasColumnName("StartDateTime")
                .HasConversion(
                    toDbValue => toDbValue.ToDateTimeUtc(),
                    fromDbValue => Instant.FromDateTimeUtc(fromDbValue.ToUniversalTime()));

            details.Property(d => d.EndDateTime)
                .HasColumnName("EndDateTime")
                .HasConversion(
                    toDbValue => toDbValue!.Value.ToDateTimeUtc(),
                    fromDbValue => Instant.FromDateTimeUtc(fromDbValue.ToUniversalTime()));
        }

        private static void ConfigureOperations(OwnedNavigationBuilder<ChargeLink, ChargeLinkOperation> operations)
        {
            operations.WithOwner().HasForeignKey("ChargeLinkId");

            operations.ToTable("ChargeLinkOperation");

            operations.HasKey(o => o.Id);

            operations.Property(o => o.Id).ValueGeneratedNever();
            operations.Property(o => o.CustomerProvidedId).HasColumnName("CustomerProvidedId");

            operations.Property(o => o.WriteDateTime)
                .ValueGeneratedOnAdd()
                .HasColumnName("WriteDateTime")
                .HasConversion(
                    toDbValue => toDbValue!.Value.ToDateTimeUtc(),
                    fromDbValue => Instant.FromDateTimeUtc(fromDbValue.ToUniversalTime()));

            operations.Property(o => o.CorrelationId).HasColumnName("CorrelationId");
        }
    }
}
