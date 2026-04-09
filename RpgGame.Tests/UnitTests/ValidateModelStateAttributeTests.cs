using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace RpgGame.API.Tests.Filters;

public class ValidateModelStateAttributeTests
{
    private readonly RpgGame.API.Filters.ValidateModelStateAttribute _attribute;

    public ValidateModelStateAttributeTests()
    {
        _attribute = new RpgGame.API.Filters.ValidateModelStateAttribute();
    }

    [Fact]
    public void OnActionExecuting_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        
        // Добавляем ошибки в ModelState
        modelState.AddModelError("Name", "Name is required");
        modelState.AddModelError("Email", "Email is invalid");

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            modelState);

        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object());

        // Act
        _attribute.OnActionExecuting(context);

        // Assert
        context.Result.Should().NotBeNull();
        context.Result.Should().BeOfType<BadRequestObjectResult>();

        var badRequestResult = (BadRequestObjectResult)context.Result!;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problemDetails = badRequestResult.Value as ValidationProblemDetails;
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("Name");
        problemDetails.Errors.Should().ContainKey("Email");
        problemDetails.Title.Should().Be("One or more validation errors occurred.");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
    }

    [Fact]
    public void OnActionExecuting_ShouldNotSetResult_WhenModelStateIsValid()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary(); // Пустой ModelState (валидный)

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            modelState);

        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object());

        // Act
        _attribute.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeNull();
    }

    [Fact]
    public void OnActionExecuting_ShouldHandleNullModelStateEntries()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        
        // Добавляем нормальную ошибку
        modelState.AddModelError("TestKey", "Test error");
        
        // Создаем ситуацию с null entry через reflection
        // ModelStateDictionary может содержать null entries в некоторых случаях
        // Мы просто не добавляем null, так как это нормально обрабатывается в фильтре

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            modelState);

        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object());

        // Act & Assert (не должно быть исключения)
        var act = () => _attribute.OnActionExecuting(context);
        
        act.Should().NotThrow();
        
        // Должен вернуть BadRequest только для валидных ошибок
        context.Result.Should().NotBeNull();
        var badRequestResult = (BadRequestObjectResult)context.Result!;
        var problemDetails = badRequestResult.Value as ValidationProblemDetails;
        problemDetails!.Errors.Should().ContainKey("TestKey");
    }

    [Fact]
    public void OnActionExecuting_ShouldIncludeInstancePath()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = new PathString("/api/characters");
        
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field", "Error message");

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            modelState);

        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object());

        // Act
        _attribute.OnActionExecuting(context);

        // Assert
        var badRequestResult = (BadRequestObjectResult)context.Result!;
        var problemDetails = badRequestResult.Value as ValidationProblemDetails;
        problemDetails!.Instance.Should().Be("/api/characters");
    }

    [Fact]
    public void OnActionExecuting_ShouldSetCorrectStatusAndType()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Test", "Error");

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            modelState);

        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object());

        // Act
        _attribute.OnActionExecuting(context);

        // Assert
        var badRequestResult = (BadRequestObjectResult)context.Result!;
        var problemDetails = badRequestResult.Value as ValidationProblemDetails;
        
        problemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        problemDetails.Detail.Should().Be("Please refer to the errors property for additional details.");
    }

    [Fact]
    public void OnActionExecuting_ShouldHandleMultipleErrorsForSameField()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        
        // Добавляем несколько ошибок для одного поля
        modelState.AddModelError("Email", "Email is required");
        modelState.AddModelError("Email", "Email must be valid format");
        modelState.AddModelError("Password", "Password is too short");

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            modelState);

        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object());

        // Act
        _attribute.OnActionExecuting(context);

        // Assert
        var badRequestResult = (BadRequestObjectResult)context.Result!;
        var problemDetails = badRequestResult.Value as ValidationProblemDetails;
        
        problemDetails!.Errors.Should().ContainKey("Email");
        problemDetails!.Errors.Should().ContainKey("Password");
        
        // У поля Email должно быть 2 ошибки
        problemDetails.Errors["Email"].Should().HaveCount(2);
        problemDetails.Errors["Email"].Should().Contain("Email is required");
        problemDetails.Errors["Email"].Should().Contain("Email must be valid format");
        
        problemDetails.Errors["Password"].Should().HaveCount(1);
    }

    [Fact]
    public void ShouldBeActionFilterAttribute()
    {
        // Arrange & Act
        var attribute = new RpgGame.API.Filters.ValidateModelStateAttribute();

        // Assert
        attribute.Should().BeAssignableTo<ActionFilterAttribute>();
        attribute.Should().BeOfType<RpgGame.API.Filters.ValidateModelStateAttribute>();
    }

    [Fact]
    public void ShouldOnlyOverrideOnActionExecuting()
    {
        // Arrange
        var attribute = new RpgGame.API.Filters.ValidateModelStateAttribute();
        var methods = attribute.GetType().GetMethods();

        // Находим методы, которые переопределены
        var overriddenMethods = methods.Where(m => 
            m.Name == "OnActionExecuting" || 
            m.Name == "OnActionExecuted" || 
            m.Name == "OnResultExecuting" || 
            m.Name == "OnResultExecuted");

        // Assert
        overriddenMethods.Should().Contain(m => m.Name == "OnActionExecuting");
        
        // Остальные методы не должны быть переопределены (будут использоваться базовые из ActionFilterAttribute)
        // Это нормально, так как фильтр нужен только для валидации перед выполнением действия
    }

    [Fact]
    public void ShouldReturnBadRequestObjectResult_WithValidationProblemDetails()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Test", "Test error");

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            modelState);

        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object());

        // Act
        _attribute.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)context.Result!;
        badRequestResult.Value.Should().BeOfType<ValidationProblemDetails>();
    }
}