using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using RpgGame.API.Middleware;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RpgGame.Tests.UnitTests
{
    public class ServerIdMiddlewareTests
    {
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly ServerIdMiddleware _middleware;
        private readonly DefaultHttpContext _httpContext;

        public ServerIdMiddlewareTests()
        {
            _nextMock = new Mock<RequestDelegate>();
            _middleware = new ServerIdMiddleware(_nextMock.Object);
            _httpContext = new DefaultHttpContext();
            _httpContext.Response.Body = new System.IO.MemoryStream();
        }

        [Fact]
        public async Task InvokeAsync_ShouldAddXServerIdHeader()
        {
            // Arrange
            var nextCalled = false;
            _nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>()))
                .Callback<HttpContext>(c => nextCalled = true)
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            nextCalled.Should().BeTrue();
            _httpContext.Response.Headers.Should().ContainKey("X-Server-Id");
            _httpContext.Response.Headers["X-Server-Id"].ToString().Should().Be(Environment.MachineName);
        }

        [Fact]
        public async Task InvokeAsync_ShouldNotOverwriteExistingHeader()
        {
            // Arrange
            const string customValue = "custom-server-id";
            _httpContext.Response.Headers.Append("X-Server-Id", customValue);
            _nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.Headers["X-Server-Id"].ToString().Should().Be(customValue);
        }

        [Fact]
        public async Task InvokeAsync_ShouldCallNextMiddleware()
        {
            // Arrange
            var nextCalled = false;
            _nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>()))
                .Callback<HttpContext>(c => nextCalled = true)
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            nextCalled.Should().BeTrue();
        }
    }
}