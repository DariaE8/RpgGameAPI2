using AutoMapper;
using RpgGame.Core.DTOs;
using RpgGame.Core.Models;

namespace RpgGame.Services.Mappings
{
    public class GameMappingProfile : Profile
    {
        public GameMappingProfile()
        {
            // Player mappings
            CreateMap<Player, PlayerDto>()
                .ForMember(dest => dest.CurrentLocation, opt => opt.MapFrom(src => 
                    src.CurrentGameLocation != null ? src.CurrentGameLocation.Name : "Unknown Location"))
                .ForMember(dest => dest.CompletedQuestsCount, opt => opt.MapFrom(src => src.CompletedQuests.Count))
                .ForMember(dest => dest.InventoryItemsCount, opt => opt.MapFrom(src => src.InventoryItems.Count))
                .ForMember(dest => dest.DefeatedEnemiesCount, opt => opt.MapFrom(src => src.DefeatedEnemies.Count))
                .ForMember(dest => dest.CompletedQuestIds, opt => opt.Ignore()) 
    .ForMember(dest => dest.InventoryItemIds, opt => opt.Ignore())
    .ForMember(dest => dest.DefeatedEnemyIds, opt => opt.Ignore());

                
            CreateMap<CreatePlayerDto, Player>()
                .ForMember(dest => dest.LocationId, opt => opt.Ignore())
                .ForMember(dest => dest.Level, opt => opt.MapFrom(_ => 1))
                .ForMember(dest => dest.Experience, opt => opt.MapFrom(_ => 0))
                .ForMember(dest => dest.Health, opt => opt.MapFrom(_ => 100))
                .ForMember(dest => dest.MaxHealth, opt => opt.MapFrom(_ => 100))
                .ForMember(dest => dest.Attack, opt => opt.MapFrom(_ => 10))
                .ForMember(dest => dest.Gold, opt => opt.MapFrom(_ => 50))
                .ForMember(dest => dest.CurrentGameLocation, opt => opt.Ignore())        
                .ForMember(dest => dest.CompletedQuests, opt => opt.Ignore())            
                .ForMember(dest => dest.DefeatedEnemies, opt => opt.Ignore())            
                .ForMember(dest => dest.InventoryItems, opt => opt.Ignore())             
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Enemy mappings  
            CreateMap<Enemy, EnemyDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => 
                    src.GameLocation != null ? src.GameLocation.Name : "Unknown Location"));
                    
            CreateMap<CreateEnemyDto, Enemy>()
                .ForMember(dest => dest.LocationId, opt => opt.Ignore())
                .ForMember(dest => dest.GameLocation, opt => opt.Ignore())              
                .ForMember(dest => dest.DefeatedByPlayers, opt => opt.Ignore())         
                .ForMember(dest => dest.RequiredForQuests, opt => opt.Ignore())         
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Quest mappings
            CreateMap<Quest, QuestDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.RequiredLocation, opt => opt.MapFrom(src => 
                    src.GameLocation != null ? src.GameLocation.Name : "Unknown Location"))
                    .ForMember(dest => dest.RequiredItemIds, opt => opt.Ignore()) 
    .ForMember(dest => dest.RequiredEnemyTypes, opt => opt.Ignore());        
                    
            CreateMap<CreateQuestDto, Quest>()
                .ForMember(dest => dest.LocationId, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentCount, opt => opt.MapFrom(_ => 0))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => QuestStatus.Available))
                .ForMember(dest => dest.GameLocation, opt => opt.Ignore())               
                .ForMember(dest => dest.PlayersCompleted, opt => opt.Ignore())           
                .ForMember(dest => dest.RequiredEnemies, opt => opt.Ignore())            
                .ForMember(dest => dest.RequiredItems, opt => opt.Ignore())              
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // GameLocation mappings
            CreateMap<GameLocation, GameLocationDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.AvailableEnemies, opt => opt.MapFrom(src =>
        src.Enemies.Select(e => e.Name).ToList()))                 
    .ForMember(dest => dest.AvailableQuests, opt => opt.MapFrom(src =>
        src.Quests.Select(q => q.Id).ToList()));            
                
            CreateMap<CreateGameLocationDto, GameLocation>()
                .ForMember(dest => dest.Enemies, opt => opt.Ignore())                    
                .ForMember(dest => dest.Quests, opt => opt.Ignore())                     
                .ForMember(dest => dest.Players, opt => opt.Ignore())                    
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Update DTO mappings
            CreateMap<UpdatePlayerDto, Player>()
                .ForMember(dest => dest.Experience, opt => opt.Ignore())
                .ForMember(dest => dest.LocationId, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentGameLocation, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedQuests, opt => opt.Ignore())
                .ForMember(dest => dest.DefeatedEnemies, opt => opt.Ignore())
                .ForMember(dest => dest.InventoryItems, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateEnemyDto, Enemy>()
                .ForMember(dest => dest.LocationId, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.GameLocation, opt => opt.Ignore())
                .ForMember(dest => dest.DefeatedByPlayers, opt => opt.Ignore())
                .ForMember(dest => dest.RequiredForQuests, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateQuestDto, Quest>()
                .ForMember(dest => dest.LocationId, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentCount, opt => opt.Ignore()) 
    .ForMember(dest => dest.Status, opt => opt.Ignore())    
                .ForMember(dest => dest.GameLocation, opt => opt.Ignore())
                .ForMember(dest => dest.PlayersCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.RequiredEnemies, opt => opt.Ignore())
                .ForMember(dest => dest.RequiredItems, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateGameLocationDto, GameLocation>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Enemies, opt => opt.Ignore())
                .ForMember(dest => dest.Quests, opt => opt.Ignore())
                .ForMember(dest => dest.Players, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}