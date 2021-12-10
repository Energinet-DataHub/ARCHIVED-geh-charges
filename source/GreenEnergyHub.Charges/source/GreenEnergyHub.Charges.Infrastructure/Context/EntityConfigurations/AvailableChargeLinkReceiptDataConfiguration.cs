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

using GreenEnergyHub.Charges.Domain.AvailableChargeLinkReceiptData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenEnergyHub.Charges.Infrastructure.Context.EntityConfigurations
{
    public class AvailableChargeLinkReceiptDataConfiguration : IEntityTypeConfiguration<AvailableChargeLinkReceiptData>
    {
        public void Configure(EntityTypeBuilder<AvailableChargeLinkReceiptData> builder)
        {
            builder.ToTable(nameof(AvailableChargeLinkReceiptData), DatabaseSchemaNames.MessageHub);
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.Property(x => x.RecipientId);
            builder.Property(x => x.RecipientRole);
            builder.Property(x => x.BusinessReasonCode);
            builder.Property(x => x.ReceiptStatus);
            builder.Property(x => x.OriginalOperationId);
            builder.Property(x => x.MeteringPointId);
            builder.Property(x => x.RequestDateTime);
            builder.Property(x => x.AvailableDataReferenceId);

            builder.Ignore(c => c.ReasonCodes);
            builder.OwnsMany<AvailableChargeLinkReceiptDataReasonCode>("_reasonCodes", ConfigureReasonCodes);
        }

        private static void ConfigureReasonCodes(
            OwnedNavigationBuilder<AvailableChargeLinkReceiptData,
            AvailableChargeLinkReceiptDataReasonCode> reasonCodes)
        {
            reasonCodes.WithOwner().HasForeignKey("AvailableChargeLinkReceiptDataId");
            reasonCodes.ToTable(nameof(AvailableChargeLinkReceiptDataReasonCode), DatabaseSchemaNames.MessageHub);
            reasonCodes.HasKey(r => r.Id);

            reasonCodes.Property(r => r.Id).ValueGeneratedNever();
            reasonCodes.Property(r => r.ReasonCode);
            reasonCodes.Property(r => r.Text);
        }
    }
}
