using Microsoft.EntityFrameworkCore;
using RpgGame.Core.Models;

namespace RpgGame.Infrastructure.Data
{
    public static class EntityRelationshipsConfiguration
    {
        public static void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            ConfigurePlayerRelationships(modelBuilder);
            ConfigureQuestRelationships(modelBuilder);
            ConfigureLocationRelationships(modelBuilder);
        }

        private static void ConfigurePlayerRelationships(ModelBuilder modelBuilder)
        {
            // Player -> Quest (Many-to-Many через PlayerQuest)
            modelBuilder.Entity<Player>()
                .HasMany(p => p.CompletedQuests)
                .WithMany(q => q.PlayersCompleted)
                .UsingEntity<Dictionary<string, object>>(
                    "PlayerQuest",
                    j => j
                        .HasOne<Quest>()
                        .WithMany()
                        .HasForeignKey("QuestId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Player>()
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("PlayerId", "QuestId");
                        j.HasIndex("QuestId");
                        j.Property<DateTime>("CompletedAt")
                            .HasDefaultValueSql("GETUTCDATE()");
                    });

            // Player -> Enemy (Many-to-Many через PlayerEnemy)
            modelBuilder.Entity<Player>()
                .HasMany(p => p.DefeatedEnemies)
                .WithMany(e => e.DefeatedByPlayers)
                .UsingEntity<Dictionary<string, object>>(
                    "PlayerEnemy",
                    j => j
                        .HasOne<Enemy>()
                        .WithMany()
                        .HasForeignKey("EnemyId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Player>()
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("PlayerId", "EnemyId");
                        j.HasIndex("EnemyId");
                        j.Property<DateTime>("DefeatedAt")
                            .HasDefaultValueSql("GETUTCDATE()");
                        j.Property<int>("GoldEarned");
                        j.Property<int>("ExperienceEarned");
                    });

            // Player -> Item (Many-to-Many через Inventory)
            modelBuilder.Entity<Player>()
                .HasMany(p => p.InventoryItems)
                .WithMany(i => i.OwnedByPlayers)
                .UsingEntity<Dictionary<string, object>>(
                    "PlayerInventory",
                    j => j
                        .HasOne<Item>()
                        .WithMany()
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Player>()
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("PlayerId", "ItemId");
                        j.HasIndex("ItemId");
                        j.Property<int>("Quantity")
                            .HasDefaultValue(1);
                        j.Property<bool>("IsEquipped")
                            .HasDefaultValue(false);
                        j.Property<DateTime>("AcquiredAt")
                            .HasDefaultValueSql("GETUTCDATE()");
                    });
        }

        private static void ConfigureQuestRelationships(ModelBuilder modelBuilder)
        {
            // Quest -> Enemy (Many-to-Many через QuestEnemyRequirement)
            modelBuilder.Entity<Quest>()
                .HasMany(q => q.RequiredEnemies)
                .WithMany(e => e.RequiredForQuests)
                .UsingEntity<Dictionary<string, object>>(
                    "QuestEnemyRequirement",
                    j => j
                        .HasOne<Enemy>()
                        .WithMany()
                        .HasForeignKey("EnemyId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Quest>()
                        .WithMany()
                        .HasForeignKey("QuestId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("QuestId", "EnemyId");
                        j.HasIndex("EnemyId");
                        j.Property<int>("RequiredCount")
                            .HasDefaultValue(1);
                    });

            // Quest -> Item (Many-to-Many через QuestItemRequirement)
            modelBuilder.Entity<Quest>()
                .HasMany(q => q.RequiredItems)
                .WithMany(i => i.RequiredForQuests)
                .UsingEntity<Dictionary<string, object>>(
                    "QuestItemRequirement",
                    j => j
                        .HasOne<Item>()
                        .WithMany()
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Quest>()
                        .WithMany()
                        .HasForeignKey("QuestId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("QuestId", "ItemId");
                        j.HasIndex("ItemId");
                        j.Property<int>("RequiredCount")
                            .HasDefaultValue(1);
                    });
        }

        private static void ConfigureLocationRelationships(ModelBuilder modelBuilder)
        {
            // Location -> Enemy (One-to-Many)
            modelBuilder.Entity<GameLocation>()
                .HasMany(l => l.Enemies)
                .WithOne(e => e.GameLocation)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Location -> Quest (One-to-Many)
            modelBuilder.Entity<GameLocation>()
                .HasMany(l => l.Quests)
                .WithOne(q => q.GameLocation)
                .HasForeignKey(q => q.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Location -> Player (One-to-Many)
            modelBuilder.Entity<GameLocation>()
                .HasMany(l => l.Players)
                .WithOne(p => p.CurrentGameLocation)
                .HasForeignKey(p => p.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}