using System.Net;
using System.Text.Json;
using RpgGame.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace RpgGame.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var problemDetails = CreateProblemDetails(context, exception);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(problemDetails, jsonOptions);
            await context.Response.WriteAsync(json);
        }

        private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
        {
            var statusCode = GetStatusCode(exception);
            var title = GetTitle(exception);
            var detail = GetDetail(exception);

            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            problemDetails.Type = GetErrorType(exception);
            
            
            if (exception is ValidationException validationException)
            {
                problemDetails.Extensions["errors"] = validationException.Errors;
            }

            // stack trace
            if (IsDevelopmentEnvironment())
            {
                problemDetails.Extensions["traceId"] = context.TraceIdentifier;
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            context.Response.StatusCode = (int)statusCode;
            return problemDetails;
        }

        private static HttpStatusCode GetStatusCode(Exception exception) =>
            exception switch
            {
                BaseException baseException => baseException.StatusCode,
                KeyNotFoundException => HttpStatusCode.NotFound,
                ArgumentException => HttpStatusCode.BadRequest,
                InvalidOperationException => HttpStatusCode.BadRequest,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                _ => HttpStatusCode.InternalServerError
            };

        private static string GetTitle(Exception exception) =>
            exception switch
            {
                BaseException baseException => baseException.Title,
                KeyNotFoundException => "Resource not found",
                ArgumentException => "Invalid argument",
                InvalidOperationException => "Invalid operation", 
                UnauthorizedAccessException => "Unauthorized",
                _ => "Internal server error"
            };

        private static string GetDetail(Exception exception) =>
            exception switch
            {
                BaseException baseException => baseException.Detail,
                _ => exception.Message
            };

        private static string GetErrorType(Exception exception) =>
            exception switch
            {
                NotFoundException => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                ValidationException => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                ConflictException => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                UnauthorizedException => "https://tools.ietf.org/html/rfc7235#section-3.1",
                ForbiddenException => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };

        private static bool IsDevelopmentEnvironment()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return environment == Environments.Development;
        }
    }
}