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
using GreenEnergyHub.Charges.Domain.AvailableChargeLinkReceiptData;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenEnergyHub.Charges.Infrastructure.Context.EntityConfigurations
{
    public class AvailableChargeLinkReceiptDataConfiguration : IEntityTypeConfiguration<AvailableChargeLinkReceiptData>
    {
        public void Configure(EntityTypeBuilder<AvailableChargeLinkReceiptData> builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.ToTable("AvailableChargeLinkReceiptData", "MessageHub");
            builder.HasKey(x => x.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.Property(x => x.RecipientId).HasColumnName("RecipientId");
            builder.Property(x => x.RecipientRole).HasColumnName("RecipientRole")
                .HasConversion(
                    toDbValue => (int)toDbValue,
                    fromDbValue => (MarketParticipantRole)fromDbValue);
            builder
                .Property(x => x.BusinessReasonCode)
                .HasColumnName("BusinessReasonCode")
                .HasConversion(
                    toDbValue => (int)toDbValue,
                    fromDbValue => (BusinessReasonCode)fromDbValue);
            builder
                .Property(x => x.ReceiptStatus)
                .HasColumnName("ReceiptStatus")
                .HasConversion(
                    toDbValue => (int)toDbValue,
                    fromDbValue => (ReceiptStatus)fromDbValue);
            builder.Property(x => x.OriginalOperationId).HasColumnName("OriginalOperationId");
            builder.Property(x => x.MeteringPointId).HasColumnName("MeteringPointId");
            builder.Property(x => x.RequestDateTime).HasColumnName("RequestTime");
            builder
                .Property(x => x.AvailableDataReferenceId)
                .HasColumnName("AvailableDataReferenceId");
            builder.Ignore(c => c.ReasonCodes);
            builder
                .OwnsMany<AvailableChargeLinkReceiptDataReasonCode>("_reasonCodes", ConfigureReasonCodes);
        }

        private static void ConfigureReasonCodes(
            OwnedNavigationBuilder<AvailableChargeLinkReceiptData,
            AvailableChargeLinkReceiptDataReasonCode> reasonCodes)
        {
            reasonCodes.WithOwner().HasForeignKey("AvailableChargeLinkReceiptDataId");
            reasonCodes.ToTable("AvailableChargeLinkReceiptDataReasonCode", "MessageHub");
            reasonCodes.HasKey(r => r.Id);
            reasonCodes.Property(r => r.Id).ValueGeneratedNever();
            reasonCodes
                .Property(r => r.ReasonCode)
                .HasColumnName("ReasonCode")
                .HasConversion(
                    toDbValue => (int)toDbValue,
                    fromDbValue => (ReasonCode)fromDbValue);
            reasonCodes.Property(r => r.Text).HasColumnName("Text");
        }
    }
}
