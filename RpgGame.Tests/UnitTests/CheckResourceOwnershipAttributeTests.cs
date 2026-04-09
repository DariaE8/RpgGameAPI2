using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace RpgGame.API.Tests.Filters;

public class CheckResourceOwnershipAttributeTests
{
    [Fact]
    public void OnActionExecuting_ShouldLogAccessInformation_WhenUserIsAuthenticated()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RpgGame.API.Filters.CheckResourceOwnershipAttribute>>();
        var attribute = new RpgGame.API.Filters.CheckResourceOwnershipAttribute("Character");
        
        // Используем реальный ServiceCollection
        var services = new ServiceCollection();
        services.AddSingleton(loggerMock.Object);
        var serviceProvider = services.BuildServiceProvider();
        
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            User = CreateUserPrincipal("testuser")
        };

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };

        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>
            {
                { "id", "123" }
            },
            new object());

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Ownership check") &&
                    v.ToString()!.Contains("testuser") &&
                    v.ToString()!.Contains("Character") &&
                    v.ToString()!.Contains("123")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Once);
    }

[Fact]
public void OnActionExecuting_ShouldHandleNullIdValue()
{
    // Arrange
    var loggerMock = new Mock<ILogger<RpgGame.API.Filters.CheckResourceOwnershipAttribute>>();
    var attribute = new RpgGame.API.Filters.CheckResourceOwnershipAttribute("Character");
    
    var services = new ServiceCollection();
    services.AddSingleton(loggerMock.Object);
    var serviceProvider = services.BuildServiceProvider();
    
    var httpContext = new DefaultHttpContext
    {
        RequestServices = serviceProvider,
        User = CreateUserPrincipal("testuser")
    };

    var actionContext = new ActionContext
    {
        HttpContext = httpContext,
        RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
        ActionDescriptor = new ActionDescriptor()
    };

    var context = new ActionExecutingContext(
        actionContext,
        new List<IFilterMetadata>(),
        new Dictionary<string, object?>
        {
            { "id", null } // null значение
        },
        new object());

    // Act
    attribute.OnActionExecuting(context);

    // Assert
    loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => 
                v.ToString()!.Contains("(null)") || // Ищем (null) в логе
                v.ToString()!.Contains("null")),    // или просто null
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
        Times.Once);
}

    [Fact]
    public void OnActionExecuting_ShouldHandleDifferentParameterNames()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RpgGame.API.Filters.CheckResourceOwnershipAttribute>>();
        var attribute = new RpgGame.API.Filters.CheckResourceOwnershipAttribute("Character");
        
        var services = new ServiceCollection();
        services.AddSingleton(loggerMock.Object);
        var serviceProvider = services.BuildServiceProvider();
        
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            User = CreateUserPrincipal("testuser")
        };

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };

        // Параметр называется не "id", а "characterId"
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>
            {
                { "characterId", "123" },
                { "name", "Test" }
            },
            new object());

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("unknown")), // не найдет "id"
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Once);
    }

    private static ClaimsPrincipal CreateUserPrincipal(string? username)
    {
        var claims = new List<Claim>();
        
        if (!string.IsNullOrEmpty(username))
        {
            claims.Add(new Claim(ClaimTypes.Name, username));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }
}