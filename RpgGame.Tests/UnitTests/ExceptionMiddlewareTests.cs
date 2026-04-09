using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using RpgGame.Core.Exceptions;
using System.Net;
using System.Text.Json;

namespace RpgGame.API.Tests.Middleware;

public class ExceptionMiddlewareTests
{
    private readonly Mock<ILogger<RpgGame.API.Middleware.ExceptionMiddleware>> _loggerMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly DefaultHttpContext _httpContext;

    public ExceptionMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<RpgGame.API.Middleware.ExceptionMiddleware>>();
        _envMock = new Mock<IWebHostEnvironment>();
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware_WhenNoException()
    {
        // Arrange
        var nextCalled = false;
        var middleware = new RpgGame.API.Middleware.ExceptionMiddleware(
            next: (context) => { nextCalled = true; return Task.CompletedTask; },
            logger: _loggerMock.Object,
            env: _envMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        nextCalled.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleNotFoundException()
    {
        // Arrange
        var exception = new NotFoundException("Resource not found");
        var middleware = new RpgGame.API.Middleware.ExceptionMiddleware(
            next: (context) => throw exception,
            logger: _loggerMock.Object,
            env: _envMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        _httpContext.Response.ContentType.Should().Be("application/json");

        var responseBody = await GetResponseBody();
        responseBody.Should().Contain("Resource not found");
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleValidationException()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Name is required" } },
            { "Email", new[] { "Email is invalid" } }
        };
        
        var exception = new ValidationException(errors);
        var middleware = new RpgGame.API.Middleware.ExceptionMiddleware(
            next: (context) => throw exception,
            logger: _loggerMock.Object,
            env: _envMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        
        var responseBody = await GetResponseBody();
        responseBody.Should().Contain("Validation error");
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleInvalidOperationException_AsBadRequest()
    {
        // Arrange
        // InvalidOperationException мапится на BadRequest (400) в вашем middleware
        var exception = new InvalidOperationException("Something went wrong");
        var middleware = new RpgGame.API.Middleware.ExceptionMiddleware(
            next: (context) => throw exception,
            logger: _loggerMock.Object,
            env: _envMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert - должно быть 400, а не 500
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        
        var responseBody = await GetResponseBody();
        responseBody.Should().Contain("Invalid operation");
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleGenericException_AsInternalServerError()
    {
        // Arrange
        // Обычное Exception без специфичного типа должно быть 500
        var exception = new Exception("Something went wrong");
        var middleware = new RpgGame.API.Middleware.ExceptionMiddleware(
            next: (context) => throw exception,
            logger: _loggerMock.Object,
            env: _envMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        
        var responseBody = await GetResponseBody();
        responseBody.Should().Contain("Internal server error");
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        var exception = new Exception("Test error");
        var middleware = new RpgGame.API.Middleware.ExceptionMiddleware(
            next: (context) => throw exception,
            logger: _loggerMock.Object,
            env: _envMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Test error")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Once);
    }

    [Theory]
    [InlineData("Development", true)]
    [InlineData("Production", false)]
    public async Task InvokeAsync_ShouldIncludeStackTrace_OnlyInDevelopment(string environment, bool shouldIncludeStackTrace)
    {
        // Arrange
        _envMock.Setup(x => x.EnvironmentName).Returns(environment);
        
        // Нужно также установить переменную окружения ASPNETCORE_ENVIRONMENT
        // так как ваш middleware использует Environment.GetEnvironmentVariable
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
        
        var exception = new Exception("Test error");
        var middleware = new RpgGame.API.Middleware.ExceptionMiddleware(
            next: (context) => throw exception,
            logger: _loggerMock.Object,
            env: _envMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        var responseBody = await GetResponseBody();
        var jsonDocument = JsonDocument.Parse(responseBody);
        var root = jsonDocument.RootElement;
        
        if (shouldIncludeStackTrace)
        {
            root.TryGetProperty("stackTrace", out _).Should().BeTrue();
            root.TryGetProperty("traceId", out _).Should().BeTrue();
        }
        else
        {
            root.TryGetProperty("stackTrace", out _).Should().BeFalse();
            root.TryGetProperty("traceId", out _).Should().BeFalse();
        }
        
        // Очищаем переменную окружения
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetInstanceToRequestPath()
    {
        // Arrange
        _httpContext.Request.Path = "/api/test";
        var exception = new Exception("Test error");
        var middleware = new RpgGame.API.Middleware.ExceptionMiddleware(
            next: (context) => throw exception,
            logger: _loggerMock.Object,
            env: _envMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        var responseBody = await GetResponseBody();
        var jsonDocument = JsonDocument.Parse(responseBody);
        var instance = jsonDocument.RootElement.GetProperty("instance").GetString();
        instance.Should().Be("/api/test");
    }

    [Fact]
    public async Task InvokeAsync_ShouldUseCamelCaseNaming()
    {
        // Arrange
        var exception = new Exception("Test");
        var middleware = new RpgGame.API.Middleware.ExceptionMiddleware(
            next: (context) => throw exception,
            logger: _loggerMock.Object,
            env: _envMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        var responseBody = await GetResponseBody();
        responseBody.Should().Contain("\"type\""); // camelCase
        responseBody.Should().Contain("\"title\"");
        responseBody.Should().Contain("\"detail\"");
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddErrorsToExtensions_ForValidationException()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Error 1", "Error 2" } }
        };
        
        var exception = new ValidationException(errors);
        var middleware = new RpgGame.API.Middleware.ExceptionMiddleware(
            next: (context) => throw exception,
            logger: _loggerMock.Object,
            env: _envMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        var responseBody = await GetResponseBody();
        var jsonDocument = JsonDocument.Parse(responseBody);
        var root = jsonDocument.RootElement;
        
        root.TryGetProperty("errors", out var errorsProperty).Should().BeTrue();
        errorsProperty.TryGetProperty("Field1", out var field1Errors).Should().BeTrue();
        field1Errors.GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleEmptyValidationErrors()
    {
        // Arrange
        var emptyErrors = new Dictionary<string, string[]>();
        var exception = new ValidationException(emptyErrors);
        var middleware = new RpgGame.API.Middleware.ExceptionMiddleware(
            next: (context) => throw exception,
            logger: _loggerMock.Object,
            env: _envMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
public async Task InvokeAsync_ShouldReturnCorrectErrorType_ForForbiddenException()
{
    // Arrange
    var exception = new ForbiddenException("Access denied");
    var middleware = new RpgGame.API.Middleware.ExceptionMiddleware(
        next: (context) => throw exception,
        logger: _loggerMock.Object,
        env: _envMock.Object);

    // Act
    await middleware.InvokeAsync(_httpContext);

    // Assert
    var responseBody = await GetResponseBody();
    var jsonDocument = JsonDocument.Parse(responseBody);
    var errorType = jsonDocument.RootElement.GetProperty("type").GetString();
    
    errorType.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.3");
}

[Fact]
public async Task InvokeAsync_ShouldReturnCorrectErrorType_ForUnauthorizedException()
{
    // Arrange
    var exception = new UnauthorizedException("Authentication required");
    var middleware = new RpgGame.API.Middleware.ExceptionMiddleware(
        next: (context) => throw exception,
        logger: _loggerMock.Object,
        env: _envMock.Object);

    // Act
    await middleware.InvokeAsync(_httpContext);

    // Assert
    var responseBody = await GetResponseBody();
    var jsonDocument = JsonDocument.Parse(responseBody);
    var errorType = jsonDocument.RootElement.GetProperty("type").GetString();
    
    errorType.Should().Be("https://tools.ietf.org/html/rfc7235#section-3.1");
}

[Fact]
public async Task InvokeAsync_ShouldReturnCorrectErrorType_ForConflictException()
{
    // Arrange
    var exception = new ConflictException("Resource already exists");
    var middleware = new RpgGame.API.Middleware.ExceptionMiddleware(
        next: (context) => throw exception,
        logger: _loggerMock.Object,
        env: _envMock.Object);

    // Act
    await middleware.InvokeAsync(_httpContext);

    // Assert
    var responseBody = await GetResponseBody();
    var jsonDocument = JsonDocument.Parse(responseBody);
    var errorType = jsonDocument.RootElement.GetProperty("type").GetString();
    
    errorType.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.8");
}

    [Theory]
    [InlineData(typeof(KeyNotFoundException), HttpStatusCode.NotFound, "Resource not found")]
    [InlineData(typeof(ArgumentException), HttpStatusCode.BadRequest, "Invalid argument")]
    [InlineData(typeof(InvalidOperationException), HttpStatusCode.BadRequest, "Invalid operation")]
    [InlineData(typeof(UnauthorizedAccessException), HttpStatusCode.Unauthorized, "Unauthorized")]
    public async Task InvokeAsync_ShouldMapStandardExceptionsCorrectly(
        Type exceptionType, HttpStatusCode expectedStatusCode, string expectedTitle)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test message")!;
        var middleware = new RpgGame.API.Middleware.ExceptionMiddleware(
            next: (context) => throw exception,
            logger: _loggerMock.Object,
            env: _envMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)expectedStatusCode);
        
        var responseBody = await GetResponseBody();
        responseBody.Should().Contain(expectedTitle);
    }

    private async Task<string> GetResponseBody()
    {
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(_httpContext.Response.Body);
        return await reader.ReadToEndAsync();
    }
}