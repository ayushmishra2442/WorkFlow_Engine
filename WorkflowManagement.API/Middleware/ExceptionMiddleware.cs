using System.Net;
using System.Text.Json;
using WorkflowManagement.API.Models;

namespace WorkflowManagement.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<ExceptionMiddleware>
            _logger;

        private readonly IWebHostEnvironment
            _environment;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;

            _logger = logger;

            _environment = environment;
        }

        public async Task InvokeAsync(
            HttpContext context)
        {
            try
            {
                await _next(context);
            }

            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An unexpected error occurred.");

                var (
                    statusCode,
                    message) =
                        GetExceptionDetails(ex);

                // Show actual exception message
                // only in Development environment
                if (_environment.IsDevelopment())
                {
                    message = ex.Message;
                }

                context.Response.ContentType =
                    "application/json";

                context.Response.StatusCode =
                    statusCode;

                var response =
                    new ApiResponse<object>(
                        message);

                var jsonResponse =
                    JsonSerializer.Serialize(
                        response);

                await context.Response
                    .WriteAsync(jsonResponse);
            }
        }

        private static (
            int StatusCode,
            string Message)
            GetExceptionDetails(
                Exception exception)
        {
            return exception switch
            {
                KeyNotFoundException =>
                    (
                        StatusCodes.Status404NotFound,
                        "Resource not found."
                    ),

                ArgumentException =>
                    (
                        StatusCodes.Status400BadRequest,
                        "Invalid request."
                    ),

                UnauthorizedAccessException =>
                    (
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized access."
                    ),

                _ =>
                    (
                        StatusCodes
                            .Status500InternalServerError,

                        "Something went wrong."
                    )
            };
        }
    }

    public static class
        ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder
            UseExceptionMiddleware(
                this IApplicationBuilder app)
        {
            return app.UseMiddleware<
                ExceptionMiddleware>();
        }
    }
}