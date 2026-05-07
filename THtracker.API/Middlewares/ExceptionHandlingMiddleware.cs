using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace THtracker.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ValidationException vex)
        {
            _logger.LogWarning(vex, "Error de validación");
            await HandleValidationExceptionAsync(context, vex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió una excepción no controlada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var errors = exception.Errors
            .GroupBy(e => string.IsNullOrEmpty(e.PropertyName) ? "_" : e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation",
            Detail = "Uno o más campos no cumplen las reglas de validación.",
            Type = "https://thtracker.com/errors/validation",
            Instance = context.Request.Path
        };
        problemDetails.Extensions["errors"] = errors;

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";
        
        var statusCode = exception switch
        {
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode == (int)HttpStatusCode.InternalServerError ? "Internal Server Error" : "Error del Sistema",
            Detail = exception.Message,
            Type = "https://thtracker.com/errors/server-error",
            Instance = context.Request.Path
        };

        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            var env = context.RequestServices.GetService(typeof(Microsoft.AspNetCore.Hosting.IWebHostEnvironment)) as Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
            problemDetails.Detail = (env?.EnvironmentName == "Development" || env?.EnvironmentName == "Testing")
                ? exception.Message
                : "Ocurrió un error inesperado en el servidor.";
        }

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
