using FluentAssertions;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace RpgGame.API.Tests.Filters;

public class AddResponseHeadersFilterTests
{
    [Fact]
    public void Apply_ShouldAddStandardResponses_WhenOperationHasNoResponses()
    {
        // Arrange
        var filter = new RpgGame.API.Filters.AddResponseHeadersFilter();
        var operation = new OpenApiOperation();
        
        // Создаем контекст используя reflection для обхода ошибок компиляции
        var context = CreateOperationFilterContext();

        // Act
        filter.Apply(operation, context);

        // Assert
        operation.Responses.Should().NotBeNull();
        operation.Responses.Should().ContainKey("200");
        operation.Responses.Should().ContainKey("400");
        operation.Responses.Should().ContainKey("404");
        operation.Responses.Should().ContainKey("500");
    }

    [Fact]
    public void Apply_ShouldAddXServerIdHeaderToAllResponses()
    {
        // Arrange
        var filter = new RpgGame.API.Filters.AddResponseHeadersFilter();
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext();

        // Act
        filter.Apply(operation, context);

        // Assert
        operation.Responses.Should().NotBeNull();
        foreach (var response in operation.Responses)
        {
            response.Value.Headers.Should().ContainKey("X-Server-Id");
            var header = response.Value.Headers["X-Server-Id"];
            header.Description.Should().Be("Идентификатор сервера (контейнера), обработавшего запрос");
            header.Schema.Type.Should().Be("string");
        }
    }

    private static OperationFilterContext CreateOperationFilterContext()
    {
        var methodInfo = typeof(object).GetMethod("ToString")!;
        var schemaGenerator = new Mock<ISchemaGenerator>().Object;
        
        // Получаем все конструкторы
        var constructors = typeof(OperationFilterContext).GetConstructors();
        
        // Пробуем разные комбинации параметров
        foreach (var constructor in constructors)
        {
            var parameters = constructor.GetParameters();
            
            if (parameters.Length == 4)
            {
                try
                {
                    // Пробуем передать параметры в правильном порядке
                    return (OperationFilterContext)constructor.Invoke(new object?[] 
                    { 
                        null,               // apiDescription
                        schemaGenerator,    // schemaGenerator
                        null,               // schemaRepository
                        methodInfo          // method
                    });
                }
                catch
                {
                    continue;
                }
            }
        }
        
        // Если не нашли подходящий конструктор, создаем mock
        return CreateMockOperationFilterContext(methodInfo, schemaGenerator);
    }
    
    private static OperationFilterContext CreateMockOperationFilterContext(MethodInfo methodInfo, ISchemaGenerator schemaGenerator)
    {
        // Создаем mock с использованием reflection
        var mockType = typeof(Mock<>).MakeGenericType(typeof(OperationFilterContext));
        var mock = Activator.CreateInstance(mockType) as Mock<OperationFilterContext>;
        
        if (mock != null)
        {
            mock.SetupGet(x => x.MethodInfo).Returns(methodInfo);
            mock.SetupGet(x => x.SchemaGenerator).Returns(schemaGenerator);
            return mock.Object;
        }
        
        // Запасной вариант
        throw new InvalidOperationException("Cannot create OperationFilterContext for testing");
    }
}