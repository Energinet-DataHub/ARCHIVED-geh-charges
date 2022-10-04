using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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
        public virtual DbSet<ChargePeriod> ChargePeriods { get; set; }
        public virtual DbSet<ChargePoint> ChargePoints { get; set; }
        public virtual DbSet<DefaultChargeLink> DefaultChargeLinks { get; set; }
        public virtual DbSet<GridAreaLink> GridAreaLinks { get; set; }
        public virtual DbSet<MarketParticipant> MarketParticipants { get; set; }
        public virtual DbSet<MeteringPoint> MeteringPoints { get; set; }
        public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

            modelBuilder.Entity<ChargePeriod>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .IsClustered(false);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Charge)
                    .WithMany(p => p.ChargePeriods)
                    .HasForeignKey(d => d.ChargeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChargePeriod_Charge");
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

            modelBuilder.Entity<GridAreaLink>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .IsClustered(false);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.GridAreaLinks)
                    .HasForeignKey(d => d.OwnerId)
                    .HasConstraintName("FK_GridAreaLink_MarketParticipant");
            });

            modelBuilder.Entity<MarketParticipant>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .IsClustered(false);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ActorId).HasDefaultValueSql("(newid())");
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

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .IsClustered(false);

                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
