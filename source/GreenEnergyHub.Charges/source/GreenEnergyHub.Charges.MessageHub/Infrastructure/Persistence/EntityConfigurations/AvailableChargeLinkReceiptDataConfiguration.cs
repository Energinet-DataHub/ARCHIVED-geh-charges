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

using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Persistence.EntityConfigurations
{
    public class AvailableChargeLinkReceiptDataConfiguration : IEntityTypeConfiguration<AvailableChargeLinksReceiptData>
    {
        public void Configure(EntityTypeBuilder<AvailableChargeLinksReceiptData> builder)
        {
            builder.ToTable(nameof(AvailableChargeLinksReceiptData));

            builder.HasKey(x => x.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.Property(x => x.SenderId);
            builder.Property(x => x.SenderRole);
            builder.Property(x => x.RecipientId);
            builder.Property(x => x.RecipientRole);
            builder.Property(x => x.BusinessReasonCode);
            builder.Property(x => x.ReceiptStatus);
            builder.Property(x => x.OriginalOperationId);
            builder.Property(x => x.MeteringPointId);
            builder.Property(x => x.RequestDateTime);
            builder.Property(x => x.AvailableDataReferenceId);
            builder.Property(x => x.DocumentType);
            builder.Property(x => x.OperationOrder);
            builder.Property(x => x.ActorId);
            builder.Ignore(x => x.ValidationErrors);
            builder.OwnsMany<AvailableReceiptValidationError>("_validationErrors", ConfigureReasonCodes);
        }

        private static void ConfigureReasonCodes(
            OwnedNavigationBuilder<AvailableChargeLinksReceiptData, AvailableReceiptValidationError> validationErrors)
        {
            validationErrors.WithOwner().HasForeignKey("AvailableChargeLinkReceiptDataId");
            validationErrors.ToTable("AvailableChargeLinksReceiptValidationError", DatabaseSchemaNames.MessageHub);
            validationErrors.HasKey(r => r.Id);
            validationErrors.Property(r => r.Id).ValueGeneratedNever();
            validationErrors.Property(r => r.ReasonCode);
            validationErrors.Property(r => r.Text);
        }
    }
}
