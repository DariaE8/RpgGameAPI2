using Microsoft.EntityFrameworkCore;
using RpgGame.Core.Models;
using System.Reflection;

namespace RpgGame.Infrastructure.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
        {
        }

        // DbSets для основных сущностей
        public DbSet<Player> Players { get; set; }
        public DbSet<Enemy> Enemies { get; set; }
        public DbSet<Quest> Quests { get; set; }
        public DbSet<GameLocation> GameLocations { get; set; }
        public DbSet<Item> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureModels(modelBuilder);
        }

        private static void ConfigureModels(ModelBuilder modelBuilder)
        {
            // Применяем конфигурации связей
            EntityRelationshipsConfiguration.ConfigureRelationships(modelBuilder);

            // Базовая конфигурация сущностей (индексы, ограничения)
            ConfigureBaseEntities(modelBuilder);
        }

        private static void ConfigureBaseEntities(ModelBuilder modelBuilder)
        {
            // Конфигурация Player
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(p => p.Id);
                
                // Индексы
                entity.HasIndex(p => p.Email).IsUnique();
                entity.HasIndex(p => p.Name);
                entity.HasIndex(p => p.Level);
                entity.HasIndex(p => p.CreatedAt);
                entity.HasIndex(p => p.LocationId); // 🔥 ДОБАВИТЬ
                
                // Ограничения
                entity.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(p => p.Email)
                    .IsRequired()
                    .HasMaxLength(255);
                    
                    
                entity.Property(p => p.Level)
                    .IsRequired()
                    .HasDefaultValue(1);
                    
                entity.Property(p => p.Health)
                    .IsRequired()
                    .HasDefaultValue(100);
                    
                entity.Property(p => p.MaxHealth)
                    .IsRequired()
                    .HasDefaultValue(100);
                    
                entity.Property(p => p.Attack)
                    .IsRequired()
                    .HasDefaultValue(10);
                    
                entity.Property(p => p.Gold)
                    .IsRequired()
                    .HasDefaultValue(50);
                    
                entity.Property(p => p.Experience)
                    .IsRequired()
                    .HasDefaultValue(0);
            });

            // Конфигурация Enemy
            modelBuilder.Entity<Enemy>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Индексы
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Level);
                entity.HasIndex(e => e.LocationId); 
                entity.HasIndex(e => new { e.Level, e.Type }); // Составной индекс
                
                // Ограничения
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                    
                entity.Property(e => e.Level)
                    .IsRequired()
                    .HasDefaultValue(1);
                    
                entity.Property(e => e.Health)
                    .IsRequired()
                    .HasDefaultValue(50);
                    
                entity.Property(e => e.MaxHealth)
                    .IsRequired()
                    .HasDefaultValue(50);
                    
                entity.Property(e => e.Attack)
                    .IsRequired()
                    .HasDefaultValue(10);
                    
                entity.Property(e => e.ExperienceReward)
                    .IsRequired()
                    .HasDefaultValue(25);
                    
                entity.Property(e => e.GoldReward)
                    .IsRequired()
                    .HasDefaultValue(10);
            });

            // Конфигурация Quest
            modelBuilder.Entity<Quest>(entity =>
            {
                entity.HasKey(q => q.Id);
                
                // Индексы
                entity.HasIndex(q => q.Title);
                entity.HasIndex(q => q.Status);
                entity.HasIndex(q => q.ExperienceReward);
                entity.HasIndex(q => q.CreatedAt);
                entity.HasIndex(q => q.LocationId);
                
                // Ограничения
                entity.Property(q => q.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                    
                entity.Property(q => q.Description)
                    .HasMaxLength(1000);
                    
                entity.Property(q => q.Objective)
                    .IsRequired()
                    .HasMaxLength(500);
                    
                entity.Property(q => q.TargetCount)
                    .IsRequired()
                    .HasDefaultValue(3);
                    
                entity.Property(q => q.CurrentCount)
                    .IsRequired()
                    .HasDefaultValue(0);
                    
                entity.Property(q => q.ExperienceReward)
                    .IsRequired()
                    .HasDefaultValue(100);
                    
                entity.Property(q => q.GoldReward)
                    .IsRequired()
                    .HasDefaultValue(50);
                    
                entity.Property(q => q.Status)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasDefaultValue(QuestStatus.Available);
                    
                entity.Ignore(q => q.Progress);
            });

            // Конфигурация GameLocation
            modelBuilder.Entity<GameLocation>(entity =>
            {
                entity.HasKey(g => g.Id);
                
                // Индексы
                entity.HasIndex(g => g.Name).IsUnique();
                entity.HasIndex(g => g.Type);
                entity.HasIndex(g => g.RequiredLevel);
                entity.HasIndex(g => g.IsSafeZone);
                
                // Ограничения
                entity.Property(g => g.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(g => g.Description)
                    .HasMaxLength(1000);
                    
                entity.Property(g => g.Type)
                    .IsRequired()
                    .HasConversion<string>();
                    
                entity.Property(g => g.RequiredLevel)
                    .IsRequired()
                    .HasDefaultValue(1);
                    
                entity.Property(g => g.IsSafeZone)
                    .IsRequired()
                    .HasDefaultValue(false);
            });

            // Конфигурация Item
            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(i => i.Id);
                
                // Индексы
                entity.HasIndex(i => i.Name);
                entity.HasIndex(i => i.Type);
                entity.HasIndex(i => i.Value);
                
                // Ограничения
                entity.Property(i => i.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(i => i.Description)
                    .HasMaxLength(500);
                    
                entity.Property(i => i.Type)
                    .IsRequired()
                    .HasConversion<string>();
                    
                entity.Property(i => i.AttackModifier)
                    .IsRequired()
                    .HasDefaultValue(0);
                    
                entity.Property(i => i.HealthModifier)
                    .IsRequired()
                    .HasDefaultValue(0);
                    
                entity.Property(i => i.Value)
                    .IsRequired()
                    .HasDefaultValue(0);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Автоматическое обновление UpdatedAt
            var entries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                entry.Entity.UpdateTimestamps();
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}