using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace RpgGame.API.Filters
{
    public class LoggingActionFilter : IActionFilter, IResultFilter
    {
        private readonly ILogger<LoggingActionFilter> _logger;
        private Stopwatch? _stopwatch;
        private readonly List<string> _ignorePaths = new() { "/swagger", "/favicon.ico" };

        public LoggingActionFilter(ILogger<LoggingActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (ShouldIgnoreLogging(context)) return;

            _stopwatch = Stopwatch.StartNew();

            var user = context.HttpContext.User.Identity?.Name ?? "Anonymous";
            var actionName = context.ActionDescriptor.DisplayName;
            var parameters = context.ActionArguments;

            _logger.LogInformation(
                "🚀 Starting request: {User} - {ActionName} with parameters: {@Parameters}",
                user, actionName, parameters);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (ShouldIgnoreLogging(context)) return;

            // 🔥 ПРОВЕРКА НА NULL
            if (_stopwatch == null) return;
            
            var elapsedMs = _stopwatch.ElapsedMilliseconds;
            
            if (context.Exception != null)
            {
                _logger.LogError(context.Exception, 
                    "❌ Request failed after {ElapsedMs}ms", elapsedMs);
            }
            else
            {
                _logger.LogInformation("✅ Action executed in {ElapsedMs}ms", elapsedMs);
            }
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (ShouldIgnoreLogging(context)) return;

            var resultType = context.Result.GetType().Name;
            _logger.LogDebug("📝 Result type: {ResultType}", resultType);
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            if (ShouldIgnoreLogging(context)) return;

            if (_stopwatch == null) return;
            
            _stopwatch.Stop();
            var elapsedMs = _stopwatch.ElapsedMilliseconds;

            var statusCode = context.HttpContext.Response.StatusCode;
            var user = context.HttpContext.User.Identity?.Name ?? "Anonymous";
            var method = context.HttpContext.Request.Method;
            var path = context.HttpContext.Request.Path;

            _logger.LogInformation(
                "📊 Request completed: {User} - {Method} {Path} - Status: {StatusCode} - Time: {ElapsedMs}ms",
                user, method, path, statusCode, elapsedMs);
        }

        private static bool ShouldIgnoreLogging(FilterContext context)
        {
            var path = context.HttpContext.Request.Path.Value ?? "";
            return path.StartsWith("/swagger") || path == "/favicon.ico";
        }
    }
}