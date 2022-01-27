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
using System.Linq;
using GreenEnergyHub.Charges.Infrastructure.ActorRegister.Persistence.Actors;
using GreenEnergyHub.Charges.Infrastructure.ActorRegister.Persistence.GridAreaLinks;
using GreenEnergyHub.Charges.Infrastructure.ActorRegister.Persistence.GridAreas;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.ActorRegister.Persistence
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "Private setters are needed by EF Core")]
    public class ActorRegister : DbContext, IActorRegister
    {
        #nullable disable
        public ActorRegister(DbContextOptions<ActorRegister> options)
            : base(options)
        {
        }

        public IQueryable<Actor> Actors => ActorsSet.AsNoTracking();

        public IQueryable<GridArea> GridAreas => GridAreasSet.AsNoTracking();

        public IQueryable<GridAreaLink> GridAreaLinks => GridAreaLinksSet.AsNoTracking();

        private DbSet<Actor> ActorsSet { get; set; }

        private DbSet<GridArea> GridAreasSet { get; set; }

        private DbSet<GridAreaLink> GridAreaLinksSet { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");

            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.ApplyConfiguration(new ActorEntityConfiguration());
            modelBuilder.ApplyConfiguration(new GridAreaEntityConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
