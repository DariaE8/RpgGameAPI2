using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RpgGame.API.Middleware
{
    /// <summary>
    /// Middleware that adds X-Server-Id header to all HTTP responses.
    /// The header value is the machine name (container hostname).
    /// </summary>
    public class ServerIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _serverId;

        public ServerIdMiddleware(RequestDelegate next)
        {
            _next = next;
            _serverId = Environment.MachineName;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add the custom header to the response if not already present
            if (!context.Response.Headers.ContainsKey("X-Server-Id"))
            {
                context.Response.Headers.Append("X-Server-Id", _serverId);
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }
}