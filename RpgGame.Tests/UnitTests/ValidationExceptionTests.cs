using Xunit;
using RpgGame.Core.Exceptions;
using System.Collections.Generic;

namespace RpgGame.API.Tests.Core.Exceptions;

public class ValidationExceptionTests
{
    [Fact]
    public void ValidationException_ConstructorWithPropertyNameAndErrorMessage_ShouldCreateExceptionWithSingleError()
    {
        // Arrange
        var propertyName = "Email";
        var errorMessage = "Email is required";

        // Act
        var exception = new ValidationException(propertyName, errorMessage);

        // Assert
        Assert.Single(exception.Errors);
        Assert.True(exception.Errors.ContainsKey(propertyName));
        Assert.Single(exception.Errors[propertyName]);
        Assert.Equal(errorMessage, exception.Errors[propertyName][0]);
        Assert.Equal("One or more validation errors occurred.", exception.Message);
    }

    [Fact]
    public void ValidationException_ConstructorWithPropertyNameAndErrorMessage_ShouldHandleEmptyPropertyName()
    {
        // Arrange & Act
        var exception = new ValidationException("", "Error message");

        // Assert
        Assert.True(exception.Errors.ContainsKey(""));
        Assert.Single(exception.Errors[""]);
        Assert.Equal("Error message", exception.Errors[""][0]);
    }

    [Fact]
    public void ValidationException_ConstructorWithPropertyNameAndErrorMessage_ShouldHandleEmptyErrorMessage()
    {
        // Arrange & Act
        var exception = new ValidationException("Field", "");

        // Assert
        Assert.True(exception.Errors.ContainsKey("Field"));
        Assert.Single(exception.Errors["Field"]);
        Assert.Equal("", exception.Errors["Field"][0]);
    }

    [Fact]
    public void ValidationException_ConstructorWithPropertyNameAndErrorMessage_ShouldHandleNullErrorMessage()
    {
        // Arrange & Act
        var exception = new ValidationException("Field", null!);

        // Assert
        Assert.True(exception.Errors.ContainsKey("Field"));
        Assert.Single(exception.Errors["Field"]);
        Assert.Null(exception.Errors["Field"][0]);
    }
}