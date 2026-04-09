using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace RpgGame.API.Tests.Filters;

public class LoggingActionFilterTests
{
    private readonly Mock<ILogger<RpgGame.API.Filters.LoggingActionFilter>> _loggerMock;
    private readonly RpgGame.API.Filters.LoggingActionFilter _filter;

    public LoggingActionFilterTests()
    {
        _loggerMock = new Mock<ILogger<RpgGame.API.Filters.LoggingActionFilter>>();
        _filter = new RpgGame.API.Filters.LoggingActionFilter(_loggerMock.Object);
    }

    [Fact]
    public void OnActionExecuting_ShouldStartStopwatch_WhenNotIgnoredPath()
    {
        // Arrange
        var context = CreateActionExecutingContext("/api/characters");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Once);
    }

    [Theory]
    [InlineData("/swagger/index.html")]
    [InlineData("/swagger/v1/swagger.json")]
    [InlineData("/favicon.ico")]
    public void OnActionExecuting_ShouldNotLog_ForIgnoredPaths(string path)
    {
        // Arrange
        var context = CreateActionExecutingContext(path);

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Never);
    }

    [Fact]
    public void OnActionExecuted_ShouldLogError_WhenExceptionOccurred()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        var context = CreateActionExecutedContext("/api/characters", exception);

        // Запускаем таймер
        _filter.OnActionExecuting(CreateActionExecutingContext("/api/characters"));
        System.Threading.Thread.Sleep(10); // Небольшая задержка для измерения времени

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request failed")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Once);
    }

    [Fact]
    public void OnActionExecuted_ShouldLogSuccess_WhenNoException()
    {
        // Arrange
        var context = CreateActionExecutedContext("/api/characters", null);

        // Запускаем таймер
        _filter.OnActionExecuting(CreateActionExecutingContext("/api/characters"));
        System.Threading.Thread.Sleep(10);

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Action executed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Once);
    }

    [Fact]
    public void OnResultExecuted_ShouldLogRequestCompletion()
    {
        // Arrange
        var context = CreateResultExecutedContext("/api/characters", 200);

        // Запускаем таймер
        _filter.OnActionExecuting(CreateActionExecutingContext("/api/characters"));
        _filter.OnResultExecuting(CreateResultExecutingContext("/api/characters"));
        System.Threading.Thread.Sleep(10);

        // Act
        _filter.OnResultExecuted(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Request completed") &&
                    v.ToString()!.Contains("Status: 200")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Once);
    }

    [Fact]
    public void OnResultExecuting_ShouldLogResultType()
    {
        // Arrange
        var context = CreateResultExecutingContext("/api/characters");

        // Act
        _filter.OnResultExecuting(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Result type")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Once);
    }

    [Fact]
    public void OnActionExecuted_ShouldHandleNullStopwatch()
    {
        // Arrange
        var context = CreateActionExecutedContext("/api/characters", null);

        // Act (вызываем без предварительного вызова OnActionExecuting)
        _filter.OnActionExecuted(context);

        // Assert - не должно быть исключения
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Never);
    }

    [Fact]
    public void OnResultExecuted_ShouldHandleNullStopwatch()
    {
        // Arrange
        var context = CreateResultExecutedContext("/api/characters", 200);

        // Act (вызываем без предварительного вызова OnActionExecuting)
        _filter.OnResultExecuted(context);

        // Assert - не должно быть исключения
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Never);
    }

    [Fact]
    public void ShouldImplementAllFilterInterfaces()
    {
        // Arrange & Act
        var filter = new RpgGame.API.Filters.LoggingActionFilter(_loggerMock.Object);

        // Assert
        filter.Should().BeAssignableTo<IActionFilter>();
        filter.Should().BeAssignableTo<IResultFilter>();
        filter.Should().NotBeAssignableTo<IAsyncActionFilter>(); // Проверяем, что не async
    }

    [Fact]
    public void Constructor_ShouldSetLogger()
    {
        // Arrange
        var logger = new Mock<ILogger<RpgGame.API.Filters.LoggingActionFilter>>().Object;

        // Act
        var filter = new RpgGame.API.Filters.LoggingActionFilter(logger);

        // Assert - проверяем через reflection, так как поле приватное
        var loggerField = typeof(RpgGame.API.Filters.LoggingActionFilter)
            .GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        loggerField.Should().NotBeNull();
        var value = loggerField!.GetValue(filter);
        value.Should().BeSameAs(logger);
    }

    [Fact]
    public void ShouldHaveIgnorePathsList()
    {
        // Arrange
        var filter = new RpgGame.API.Filters.LoggingActionFilter(_loggerMock.Object);

        // Assert - проверяем через reflection, что список путей для игнорирования существует
        var ignorePathsField = typeof(RpgGame.API.Filters.LoggingActionFilter)
            .GetField("_ignorePaths", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        ignorePathsField.Should().NotBeNull();
        var value = ignorePathsField!.GetValue(filter) as List<string>;
        value.Should().NotBeNull();
        value!.Should().Contain("/swagger");
        value.Should().Contain("/favicon.ico");
    }

    private static ActionExecutingContext CreateActionExecutingContext(string path)
    {
        var httpContext = new DefaultHttpContext
        {
            Request = { Path = new PathString(path) },
            User = CreateUserPrincipal("testuser")
        };

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
            ActionDescriptor = new ActionDescriptor
            {
                DisplayName = "TestController.TestAction"
            }
        };

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>
            {
                { "id", "123" },
                { "name", "Test" }
            },
            new object());
    }

    private static ActionExecutedContext CreateActionExecutedContext(string path, Exception? exception)
    {
        var httpContext = new DefaultHttpContext
        {
            Request = { Path = new PathString(path) },
            User = CreateUserPrincipal("testuser")
        };

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };

        return new ActionExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            new object())
        {
            Exception = exception,
            ExceptionHandled = exception != null
        };
    }

    private static ResultExecutingContext CreateResultExecutingContext(string path)
    {
        var httpContext = new DefaultHttpContext
        {
            Request = { Path = new PathString(path) },
            User = CreateUserPrincipal("testuser")
        };

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };

        return new ResultExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new OkResult(),
            new object());
    }

    private static ResultExecutedContext CreateResultExecutedContext(string path, int statusCode)
    {
        var httpContext = new DefaultHttpContext
        {
            Request = 
            { 
                Path = new PathString(path),
                Method = "GET"
            },
            Response = { StatusCode = statusCode },
            User = CreateUserPrincipal("testuser")
        };

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };

        return new ResultExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            new OkResult(),
            new object());
    }

    private static ClaimsPrincipal CreateUserPrincipal(string username)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }
}