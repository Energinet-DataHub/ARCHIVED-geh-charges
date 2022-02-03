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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    public partial class QueryDbContext : DbContext
    {
        public QueryDbContext()
        {
        }

        public QueryDbContext(DbContextOptions<QueryDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Charge> Charges { get; set; }

        public virtual DbSet<ChargeLink> ChargeLinks { get; set; }

        public virtual DbSet<ChargePoint> ChargePoints { get; set; }

        public virtual DbSet<DefaultChargeLink> DefaultChargeLinks { get; set; }

        public virtual DbSet<GridArea> GridAreas { get; set; }

        public virtual DbSet<GridAreaLink> GridAreaLinks { get; set; }

        public virtual DbSet<MarketParticipant> MarketParticipants { get; set; }

        public virtual DbSet<MeteringPoint> MeteringPoints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Charge>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .IsClustered(false);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.Charges)
                    .HasForeignKey(d => d.OwnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Charge_MarketParticipant");
            });

            modelBuilder.Entity<ChargeLink>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .IsClustered(false);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Charge)
                    .WithMany(p => p.ChargeLinks)
                    .HasForeignKey(d => d.ChargeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChargeLink_Charge");

                entity.HasOne(d => d.MeteringPoint)
                    .WithMany(p => p.ChargeLinks)
                    .HasForeignKey(d => d.MeteringPointId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChargeLink_MeteringPoint");
            });

            modelBuilder.Entity<ChargePoint>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("PK_ChargePrice")
                    .IsClustered(false);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Charge)
                    .WithMany(p => p.ChargePoints)
                    .HasForeignKey(d => d.ChargeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChargePoint_Charge");
            });

            modelBuilder.Entity<DefaultChargeLink>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .IsClustered(false);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Charge)
                    .WithMany(p => p.DefaultChargeLinks)
                    .HasForeignKey(d => d.ChargeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DefaultChargeLink_Charge");
            });

            modelBuilder.Entity<GridArea>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .IsClustered(false);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.GridAccessProvider)
                    .WithMany(p => p.GridAreas)
                    .HasForeignKey(d => d.GridAccessProviderId)
                    .HasConstraintName("FK_GridArea_MarketParticipant");
            });

            modelBuilder.Entity<GridAreaLink>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .IsClustered(false);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.GridArea)
                    .WithMany(p => p.GridAreaLinks)
                    .HasForeignKey(d => d.GridAreaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_GridAreaLink_GridArea");
            });

            modelBuilder.Entity<MarketParticipant>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .IsClustered(false);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).HasDefaultValueSql("('')");
            });

            modelBuilder.Entity<MeteringPoint>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .IsClustered(false);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.GridAreaLink)
                    .WithMany(p => p.MeteringPoints)
                    .HasForeignKey(d => d.GridAreaLinkId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MeteringPoint_GridAreaLink");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
