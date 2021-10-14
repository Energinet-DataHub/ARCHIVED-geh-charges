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
using GreenEnergyHub.Charges.Domain.ChargeLinkHistory;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenEnergyHub.Charges.Infrastructure.Context.EntityConfigurations
{
    public class ChargeLinkTransmissionRequestConfiguration : IEntityTypeConfiguration<ChargeLinkTransmissionRequest>
    {
        public void Configure(EntityTypeBuilder<ChargeLinkTransmissionRequest> builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.ToTable("ChargeLinkTransmissionRequest", "Charges");

            builder.HasKey(x => x.Id);

            builder
                .Property(x => x.Recipient)
                .HasColumnName("Recipient");

            builder
                .Property(x => x.RecipientRole)
                .HasColumnName("RecipientRole")
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
                .Property(x => x.ChargeId)
                .HasColumnName("ChargeId");

            builder
                .Property(x => x.ChargeOwner)
                .HasColumnName("ChargeOwner");

            builder
                .Property(x => x.ChargeType)
                .HasColumnName("ChargeType")
                .HasConversion(
                    toDbValue => (int)toDbValue,
                    fromDbValue => (ChargeType)fromDbValue);

            builder
                .Property(x => x.MeteringPointId)
                .HasColumnName("MeteringPointId");

            builder
                .Property(x => x.Factor)
                .HasColumnName("Factor");

            builder
                .Property(x => x.StartDateTime)
                .HasColumnName("StartDateTime");

            builder
                .Property(x => x.EndDateTime)
                .HasColumnName("EndDateTime");

            builder
                .Property(x => x.MessageHubId)
                .HasColumnName("MessageHubId");
        }
    }
}
