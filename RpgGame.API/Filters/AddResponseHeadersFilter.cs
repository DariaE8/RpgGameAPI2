using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace RpgGame.API.Filters
{
    public class AddResponseHeadersFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Responses ??= new OpenApiResponses();

            // стандартные коды ответов
            AddOrUpdateResponse(operation, "200", "Success");
            AddOrUpdateResponse(operation, "400", "Bad Request - Validation error");
            AddOrUpdateResponse(operation, "404", "Not Found");
            AddOrUpdateResponse(operation, "500", "Internal Server Error");
        }

        private static void AddOrUpdateResponse(OpenApiOperation operation, string statusCode, string description)
        {
            if (!operation.Responses.ContainsKey(statusCode))
            {
                operation.Responses.Add(statusCode, new OpenApiResponse { Description = description });
            }

            var response = operation.Responses[statusCode];
            response.Headers ??= new Dictionary<string, OpenApiHeader>();

            // Добавляем заголовок X-Server-Id, если его ещё нет
            if (!response.Headers.ContainsKey("X-Server-Id"))
            {
                response.Headers.Add("X-Server-Id", new OpenApiHeader
                {
                    Description = "Идентификатор сервера (контейнера), обработавшего запрос",
                    Schema = new OpenApiSchema { Type = "string" }
                });
            }
        }
    }
}