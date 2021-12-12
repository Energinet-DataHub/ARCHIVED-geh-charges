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
using GreenEnergyHub.Charges.Domain.Charges;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenEnergyHub.Charges.Infrastructure.Context.EntityConfigurations
{
    public class ChargeEntityConfiguration : IEntityTypeConfiguration<Charge>
    {
        private static readonly string _aggregateTableName = nameof(Charge);

        public void Configure(EntityTypeBuilder<Charge> builder)
        {
            builder.ToTable(_aggregateTableName);
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Description);
            builder.Property(c => c.Name);
            builder.Property(c => c.OwnerId);
            builder.Property(c => c.SenderProvidedChargeId);
            builder.Property(c => c.Resolution);
            builder.Property(c => c.Type);
            builder.Property(c => c.TaxIndicator);
            builder.Property(c => c.TransparentInvoicing);
            builder.Property(c => c.VatClassification);
            builder.Property(c => c.StartDateTime);
            builder.Property(c => c.EndDateTime);

            builder.OwnsMany(c => c.Points, ConfigurePoints);

            // Enable EF Core to hydrate the points
            builder.Metadata
                .FindNavigation(nameof(Charge.Points))
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        }

        private static void ConfigurePoints(OwnedNavigationBuilder<Charge, Point> points)
        {
            // This field is defined in the SQL model (as a foreign key)
            points.WithOwner().HasForeignKey($"{_aggregateTableName}Id");

            var tableName = $"{_aggregateTableName}{nameof(Point)}";
            points.ToTable(tableName);

            // This is a database-only column - doesn't exist in domain model as point is not an aggregate
            points.Property<Guid>("Id").ValueGeneratedOnAdd();

            points.Property(p => p.Position);
            points.Property(p => p.Price);
            points.Property(p => p.Time);
        }
    }
}
