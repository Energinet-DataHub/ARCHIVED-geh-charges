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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenEnergyHub.Charges.Infrastructure.ActorRegister.Persistence.GridAreas
{
    public class GridAreaEntityConfiguration : IEntityTypeConfiguration<GridArea>
    {
        public void Configure(EntityTypeBuilder<GridArea> builder)
        {
            builder.ToView("NetArea");

            builder.HasKey(a => a.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(a => a.Id);
            builder.Property(a => a.RecordId);
            builder.Property(a => a.ActorId);
            builder.Property(a => a.Code);
            builder.Property(a => a.Active);
            builder.Property(a => a.Name);
            builder.Property(a => a.PriceAreaCode);
        }
    }
}
