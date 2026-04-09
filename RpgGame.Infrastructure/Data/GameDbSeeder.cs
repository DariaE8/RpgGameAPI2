using Microsoft.EntityFrameworkCore;
using RpgGame.Core.Models;
using RpgGame.Infrastructure.Data;

namespace RpgGame.Infrastructure.Data
{
    public static class GameDbSeeder
    {
        public static async Task SeedAsync(GameDbContext context)
        {
            // Проверяем, есть ли уже данные
            if (await context.Players.AnyAsync())
                return;

            // СОХРАНЯЕМ КАЖДУЮ ГРУППУ ДАННЫХ ОТДЕЛЬНО
            await SeedLocations(context);
            await context.SaveChangesAsync();
            
            await SeedEnemies(context);
            await context.SaveChangesAsync();
            
            await SeedQuests(context);
            await context.SaveChangesAsync();
            
            await SeedPlayers(context);
            await context.SaveChangesAsync();
            
            await SeedItems(context);
            await context.SaveChangesAsync();
        }

        private static async Task SeedLocations(GameDbContext context)
        {
            var locations = new[]
            {
                new GameLocation 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Starting Forest", 
                    Description = "A peaceful forest where new adventurers begin their journey",
                    Type = LocationType.Forest,
                    RequiredLevel = 1,
                    IsSafeZone = true
                },
                new GameLocation 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Dark Cave", 
                    Description = "A dangerous cave filled with monsters",
                    Type = LocationType.Cave,
                    RequiredLevel = 5,
                    IsSafeZone = false
                },
                new GameLocation 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Dragon Mountain", 
                    Description = "A treacherous mountain inhabited by dragons",
                    Type = LocationType.Mountain,
                    RequiredLevel = 15,
                    IsSafeZone = false
                },
                new GameLocation 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Safe Village", 
                    Description = "A peaceful village where players can rest",
                    Type = LocationType.Village,
                    RequiredLevel = 1,
                    IsSafeZone = true
                }
            };

            await context.GameLocations.AddRangeAsync(locations);
        }

        private static async Task SeedEnemies(GameDbContext context)
        {
            var forestLocation = await context.GameLocations.FirstAsync(l => l.Name == "Starting Forest");
            var caveLocation = await context.GameLocations.FirstAsync(l => l.Name == "Dark Cave");
            var mountainLocation = await context.GameLocations.FirstAsync(l => l.Name == "Dragon Mountain");

            var enemies = new[]
            {
                new Enemy 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Forest Goblin", 
                    Type = EnemyType.Goblin,
                    Level = 2,
                    Health = 30,
                    MaxHealth = 30,
                    Attack = 8,
                    ExperienceReward = 15,
                    GoldReward = 5,
                    LocationId = forestLocation.Id // 🔥 ИСПОЛЬЗУЕМ LocationId вместо Location
                },
                new Enemy 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Cave Spider", 
                    Type = EnemyType.Spider,
                    Level = 6,
                    Health = 45,
                    MaxHealth = 45,
                    Attack = 12,
                    ExperienceReward = 35,
                    GoldReward = 15,
                    LocationId = caveLocation.Id // 🔥 ИСПОЛЬЗУЕМ LocationId вместо Location
                },
                new Enemy 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Mountain Dragon", 
                    Type = EnemyType.Dragon,
                    Level = 20,
                    Health = 200,
                    MaxHealth = 200,
                    Attack = 35,
                    ExperienceReward = 150,
                    GoldReward = 100,
                    LocationId = mountainLocation.Id // 🔥 ИСПОЛЬЗУЕМ LocationId вместо Location
                },
                new Enemy 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Wild Orc", 
                    Type = EnemyType.Orc,
                    Level = 8,
                    Health = 60,
                    MaxHealth = 60,
                    Attack = 18,
                    ExperienceReward = 50,
                    GoldReward = 25,
                    LocationId = caveLocation.Id // 🔥 ИСПОЛЬЗУЕМ LocationId вместо Location
                }
            };

            await context.Enemies.AddRangeAsync(enemies);
        }

        private static async Task SeedQuests(GameDbContext context)
        {
            var forestLocation = await context.GameLocations.FirstAsync(l => l.Name == "Starting Forest");
            var caveLocation = await context.GameLocations.FirstAsync(l => l.Name == "Dark Cave");
            var mountainLocation = await context.GameLocations.FirstAsync(l => l.Name == "Dragon Mountain");

            var quests = new[]
            {
                new Quest 
                { 
                    Id = Guid.NewGuid(),
                    Title = "Goblin Hunt", 
                    Description = "Clear the forest of dangerous goblins",
                    Objective = "Defeat 3 Forest Goblins",
                    TargetCount = 3,
                    ExperienceReward = 100,
                    GoldReward = 50,
                    Status = QuestStatus.Available,
                    LocationId = forestLocation.Id // 🔥 ИСПОЛЬЗУЕМ LocationId вместо RequiredLocation
                },
                new Quest 
                { 
                    Id = Guid.NewGuid(),
                    Title = "Cave Exploration", 
                    Description = "Explore the dark cave and eliminate threats",
                    Objective = "Defeat 2 Cave Spiders",
                    TargetCount = 2,
                    ExperienceReward = 150,
                    GoldReward = 75,
                    Status = QuestStatus.Available,
                    LocationId = caveLocation.Id // 🔥 ИСПОЛЬЗУЕМ LocationId вместо RequiredLocation
                },
                new Quest 
                { 
                    Id = Guid.NewGuid(),
                    Title = "Dragon Slayer", 
                    Description = "Defeat the mighty dragon terrorizing the mountain",
                    Objective = "Defeat the Mountain Dragon",
                    TargetCount = 1,
                    ExperienceReward = 500,
                    GoldReward = 200,
                    Status = QuestStatus.Available,
                    LocationId = mountainLocation.Id // 🔥 ИСПОЛЬЗУЕМ LocationId вместо RequiredLocation
                }
            };

            await context.Quests.AddRangeAsync(quests);
        }

        private static async Task SeedPlayers(GameDbContext context)
        {
            var villageLocation = await context.GameLocations.FirstAsync(l => l.Name == "Safe Village");

            var players = new[]
            {
                new Player 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Aragorn", 
                    Email = "aragorn@middleearth.com",
                    Level = 5,
                    Experience = 250,
                    Health = 120,
                    MaxHealth = 120,
                    Attack = 15,
                    Gold = 100,
                    LocationId = villageLocation.Id // 🔥 ИСПОЛЬЗУЕМ LocationId вместо CurrentLocation
                },
                new Player 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Legolas", 
                    Email = "legolas@middleearth.com",
                    Level = 8,
                    Experience = 600,
                    Health = 90,
                    MaxHealth = 90,
                    Attack = 25,
                    Gold = 200,
                    LocationId = villageLocation.Id // 🔥 ИСПОЛЬЗУЕМ LocationId вместо CurrentLocation
                },
                new Player 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Gimli", 
                    Email = "gimli@middleearth.com",
                    Level = 6,
                    Experience = 400,
                    Health = 150,
                    MaxHealth = 150,
                    Attack = 20,
                    Gold = 150,
                    LocationId = villageLocation.Id // 🔥 ИСПОЛЬЗУЕМ LocationId вместо CurrentLocation
                }
            };

            await context.Players.AddRangeAsync(players);
        }

        private static async Task SeedItems(GameDbContext context)
        {
            var items = new[]
            {
                new Item 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Iron Sword", 
                    Description = "A basic iron sword",
                    Type = ItemType.Weapon,
                    AttackModifier = 10,
                    HealthModifier = 0,
                    Value = 50
                },
                new Item 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Steel Armor", 
                    Description = "Protective steel armor",
                    Type = ItemType.Armor,
                    AttackModifier = 0,
                    HealthModifier = 20,
                    Value = 100
                },
                new Item 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Health Potion", 
                    Description = "Restores 50 health points",
                    Type = ItemType.Consumable,
                    AttackModifier = 0,
                    HealthModifier = 50,
                    Value = 25
                }
            };

            await context.Items.AddRangeAsync(items);
        }
    }
}