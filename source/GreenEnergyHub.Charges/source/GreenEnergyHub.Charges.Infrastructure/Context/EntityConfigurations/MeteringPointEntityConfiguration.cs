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
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenEnergyHub.Charges.Infrastructure.Context.EntityConfigurations
{
    public class MeteringPointEntityConfiguration : IEntityTypeConfiguration<MeteringPoint>
    {
        public void Configure(EntityTypeBuilder<MeteringPoint> builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.ToTable("MeteringPoint", "Charges");

            builder.HasKey(x => x.RowId);

            builder
                .Property(x => x.ConnectionState)
                .HasColumnName("ConnectionState");

            builder
                .Property(x => x.EffectiveDate)
                .HasColumnName("EffectiveDate");

            builder
                .Property(x => x.SettlementMethod)
                .HasColumnName("SettlementMethod");

            builder
                .Property(x => x.GridAreaId)
                .HasColumnName("GridAreaId");

            builder
                .Property(x => x.MeteringPointId)
                .HasColumnName("MeteringPointId");

            builder
                .Property(x => x.MeteringPointType)
                .HasColumnName("MeteringPointType");
        }
    }
}
