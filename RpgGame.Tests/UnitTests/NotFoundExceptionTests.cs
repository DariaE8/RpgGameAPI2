using FluentAssertions;
using Xunit;
using RpgGame.Core.Exceptions;
using System.Net;

namespace RpgGame.API.Tests.Core.Exceptions;

public class NotFoundExceptionTests
{
    [Fact]
    public void NotFoundException_ConstructorWithEntityNameAndKey_ShouldCreateExceptionWithFormattedMessage()
    {
        // Arrange
        var entityName = "Player";
        var key = Guid.NewGuid();

        // Act
        var exception = new NotFoundException(entityName, key);

        // Assert
        exception.Message.Should().Be($"Entity '{entityName}' with key '{key}' was not found.");
    }

    [Fact]
    public void NotFoundException_ConstructorWithEntityNameAndKey_ShouldHandleDifferentKeyTypes()
    {
        // Arrange & Act - тестируем разные типы ключей
        var exception1 = new NotFoundException("Player", 123); // int key
        var exception2 = new NotFoundException("Quest", "quest-123"); // string key
        var exception3 = new NotFoundException("Enemy", Guid.NewGuid()); // Guid key

        // Assert
        exception1.Message.Should().Be("Entity 'Player' with key '123' was not found.");
        exception2.Message.Should().Be("Entity 'Quest' with key 'quest-123' was not found.");
        exception3.Message.Should().Contain("Entity 'Enemy' with key");
    }

    [Fact]
    public void NotFoundException_ConstructorWithEntityNameAndKey_ShouldHandleEmptyEntityName()
    {
        // Arrange & Act
        var exception = new NotFoundException("", 123);

        // Assert
        exception.Message.Should().Be("Entity '' with key '123' was not found.");
    }
}