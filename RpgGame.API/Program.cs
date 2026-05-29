using Microsoft.OpenApi.Models;
using RpgGame.Core.Interfaces;
using RpgGame.Services.Mappings;
using RpgGame.Core.Validators;
using RpgGame.Services.Services;
using RpgGame.Core.Models;
using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using RpgGame.Infrastructure.Data;
using RpgGame.API.Middleware;
using RpgGame.Core.Exceptions;
using RpgGame.API.Filters;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Prometheus;


var builder = WebApplication.CreateBuilder(args);

// Рега сервисов
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelStateAttribute>(); // Автомат валидация
    options.Filters.Add<LoggingActionFilter>(); // Лог всех запросов
});
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssembly(typeof(CreatePlayerDtoValidator).Assembly);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(typeof(GameMappingProfile));
builder.Services.AddHealthChecks();

// Лог
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddScoped<LoggingActionFilter>();
builder.Services.AddScoped<ValidateModelStateAttribute>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "RPG Game API", 
        Version = "v1",
        Description = "A comprehensive RPG game management API with players, quests, enemies and locations",
        Contact = new OpenApiContact
        {
            Name = "RPG Game Team",
            Email = "support@rpggame.com"
        }
    });

    c.EnableAnnotations();
    
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Рега сервисов
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IEnemyService, EnemyService>();
builder.Services.AddScoped<IQuestService, QuestService>();
builder.Services.AddScoped<IGameLocationService, GameLocationService>();

var app = builder.Build();

app.UseHttpMetrics(options =>
{
    // Заставляем prometheus-net брать реальный путь из HTTP-контекста для лейбла "endpoint"
    options.RequestCount.AdditionalLabels.Add("endpoint");
    
    // Переопределяем стандартное поведение: если маршрут пустой (404), пишем туда URL-путь запроса
    app.Use((context, next) =>
    {
        var path = context.Request.Path.Value ?? "/";
        context.Items["prometheus-net-route"] = path;
        return next();
    });
});
app.UseMiddleware<ServerIdMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.MapHealthChecks("/health");
app.MapMetrics();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<GameDbContext>();
        
        context.Database.Migrate();
        
        await GameDbSeeder.SeedAsync(context);
        
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("✅ Database migrated and seeded successfully");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ An error occurred while migrating or seeding the database");
    }
}

var appLogger = app.Services.GetRequiredService<ILogger<Program>>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RPG Game API v1");
        c.DocumentTitle = "RPG Game API Documentation";
        c.EnableDeepLinking();
        c.DisplayOperationId();
    });
    
    appLogger.LogInformation("🚀 Swagger UI available at /swagger");
}

appLogger.LogInformation("🎮 RPG Game API started successfully");

app.MapControllers();
app.Run();