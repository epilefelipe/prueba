using System.Net;
using System.Text.Json;
using FluentValidation;

namespace TicketManager.API.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthMiddleware> _logger;

        public AuthMiddleware(RequestDelegate next, ILogger<AuthMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";
            if (context.Request.Method == "OPTIONS" || path.StartsWith("/swagger"))
            {
                await _next(context);
                return;
            }

            var xUser = context.Request.Headers["X-User"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(xUser))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new { error = "X-User header is required" }));
                return;
            }

            context.Items["X-User"] = xUser;
            _logger.LogDebug("Request by {User} on {Method} {Path}", xUser, context.Request.Method, path);
            await _next(context);
        }
    }

    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error: {Errors}", ex.Errors);
                await WriteErrorResponse(context, HttpStatusCode.BadRequest, new
                {
                    error = "Validation failed",
                    details = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("transition"))
            {
                _logger.LogWarning("Status transition conflict: {Message}", ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.Conflict, new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Not found: {Message}", ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.NotFound, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteErrorResponse(context, HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred" });
            }
        }

        private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, object body)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";
            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await context.Response.WriteAsync(json);
        }
    }
}
