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
        public QueryDbContext(DbContextOptions<QueryDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Charge> Charges { get; set; }

        public virtual DbSet<ChargeLink> ChargeLinks { get; set; }

        public virtual DbSet<ChargePoint> ChargePoints { get; set; }

        public virtual DbSet<DefaultChargeLink> DefaultChargeLinks { get; set; }

        public virtual DbSet<MarketParticipant> MarketParticipants { get; set; }

        public virtual DbSet<MeteringPoint> MeteringPoints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Charge>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .IsClustered(false);

                entity.ToTable("Charge", "Charges");

                entity.HasIndex(e => new { e.SenderProvidedChargeId, e.Type, e.OwnerId }, "IX_SenderProvidedChargeId_ChargeType_MarketParticipantId");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(2048);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(132);

                entity.Property(e => e.SenderProvidedChargeId)
                    .IsRequired()
                    .HasMaxLength(35);

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.Charges)
                    .HasForeignKey(d => d.OwnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
                    /*.HasConstraintName("FK__Charge__MarketPa__534D60F1");*/
            });

            modelBuilder.Entity<ChargeLink>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .IsClustered(false);

                entity.ToTable("ChargeLink", "Charges");

                entity.HasIndex(e => new { e.MeteringPointId, e.ChargeId }, "IX_MeteringPointId_ChargeId");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Charge)
                    .WithMany(p => p.ChargeLinks)
                    .HasForeignKey(d => d.ChargeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
                /*.HasConstraintName("FK__ChargeLin__Charg__656C112C");*/

                entity.HasOne(d => d.MeteringPoint)
                    .WithMany(p => p.ChargeLinks)
                    .HasForeignKey(d => d.MeteringPointId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
                    /*.HasConstraintName("FK__ChargeLin__Meter__66603565");*/
            });

            modelBuilder.Entity<ChargePoint>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("PK_ChargePrice")
                    .IsClustered(false);

                entity.ToTable("ChargePoint", "Charges");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Position).HasDefaultValueSql("((1))");

                entity.Property(e => e.Price).HasColumnType("decimal(14, 6)");

                entity.HasOne(d => d.Charge)
                    .WithMany(p => p.ChargePoints)
                    .HasForeignKey(d => d.ChargeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Charge");
            });

            modelBuilder.Entity<DefaultChargeLink>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .IsClustered(false);

                entity.ToTable("DefaultChargeLink", "Charges");

                entity.HasIndex(e => new { e.MeteringPointType, e.StartDateTime, e.EndDateTime }, "IX_MeteringPointType_StartDateTime_EndDateTime");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Charge)
                    .WithMany(p => p.DefaultChargeLinks)
                    .HasForeignKey(d => d.ChargeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
                    /*.HasConstraintName("FK__DefaultCh__Charg__60A75C0F");*/
            });

            modelBuilder.Entity<MarketParticipant>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .IsClustered(false);

                entity.ToTable("MarketParticipant", "Charges");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.MarketParticipantId)
                    .IsRequired()
                    .HasMaxLength(35);
            });

            modelBuilder.Entity<MeteringPoint>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .IsClustered(false);

                entity.ToTable("MeteringPoint", "Charges");

                entity.HasIndex(e => e.MeteringPointId, "IX_MeteringPointId");

                entity.HasIndex(e => e.MeteringPointId, "UC_MeteringPointId")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.GridAreaId)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.MeteringPointId)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
