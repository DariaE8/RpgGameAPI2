using FluentAssertions;
using Xunit;
using RpgGame.Core.Exceptions;

namespace RpgGame.Tests.UnitTests
{
    public class UnauthorizedExceptionTests
    {
        [Fact]
        public void Constructor_ShouldCreateExceptionWithMessage()
        {
            // Arrange & Act
            var exception = new UnauthorizedException("Access denied");

            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Be("Access denied");
        }

        [Fact]
        public void Constructor_ShouldHandleEmptyMessage()
        {
            // Arrange & Act
            var exception = new UnauthorizedException("");

            // Assert
            exception.Message.Should().Be("");
        }

        [Fact]
        public void Constructor_ShouldHandleNullMessage_ByProvidingDefault()
        {
            // Arrange & Act
            // В .NET конструктор Exception не принимает null, он преобразует его
            // в дефолтное сообщение
            var exception = new UnauthorizedException(null!);

            // Assert
            // Не проверяем на null, а проверяем что сообщение не пустое
            // (в .NET будет дефолтное сообщение типа "Exception of type '...' was thrown.")
            exception.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ShouldBeThrowable()
        {
            // Act & Assert
            Action act = () => throw new UnauthorizedException("Error");
            
            act.Should().Throw<UnauthorizedException>()
                .WithMessage("Error");
        }

        [Fact]
        public void ShouldHaveCorrectType()
        {
            // Arrange & Act
            var exception = new UnauthorizedException("Test");

            // Assert
            exception.Should().BeOfType<UnauthorizedException>();
        }

        [Theory]
        [InlineData("Simple message")]
        [InlineData("Message with spaces and 123 numbers")]
        [InlineData("Special chars: !@#$%^&*()")]
        public void Constructor_ShouldAcceptVariousMessages(string message)
        {
            // Arrange & Act
            var exception = new UnauthorizedException(message);

            // Assert
            exception.Message.Should().Be(message);
        }

        [Fact]
        public void ShouldWorkWithTryCatch()
        {
            // Arrange
            var wasCaught = false;
            var caughtMessage = "";

            // Act
            try
            {
                throw new UnauthorizedException("Test message");
            }
            catch (UnauthorizedException ex)
            {
                wasCaught = true;
                caughtMessage = ex.Message;
            }

            // Assert
            wasCaught.Should().BeTrue();
            caughtMessage.Should().Be("Test message");
        }

        [Fact]
        public void ShouldHaveStandardExceptionProperties()
        {
            // Arrange & Act
            var exception = new UnauthorizedException("Test");

            // Assert
            exception.InnerException.Should().BeNull();
            exception.Data.Should().NotBeNull();
            exception.Source.Should().BeNull(); // пока не установлен
        }

        [Fact]
        public void ToString_ShouldContainExceptionInfo()
        {
            // Arrange
            var exception = new UnauthorizedException("Access denied");

            // Act
            var result = exception.ToString();

            // Assert
            result.Should().Contain("UnauthorizedException");
            result.Should().Contain("Access denied");
        }
    }
}