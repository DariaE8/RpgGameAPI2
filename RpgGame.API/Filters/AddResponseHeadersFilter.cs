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
            if (!operation.Responses.ContainsKey("200"))
            {
                operation.Responses.Add("200", new OpenApiResponse { Description = "Success" });
            }
            
            if (!operation.Responses.ContainsKey("400"))
            {
                operation.Responses.Add("400", new OpenApiResponse { Description = "Bad Request - Validation error" });
            }
            
            if (!operation.Responses.ContainsKey("404"))
            {
                operation.Responses.Add("404", new OpenApiResponse { Description = "Not Found" });
            }
            
            if (!operation.Responses.ContainsKey("500"))
            {
                operation.Responses.Add("500", new OpenApiResponse { Description = "Internal Server Error" });
            }
        }
    }
}