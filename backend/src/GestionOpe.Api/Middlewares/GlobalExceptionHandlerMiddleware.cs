using System.Net;
using System.Text.Json;
using FluentValidation;
using GestionOpe.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GestionOpe.Api.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails();

        switch (exception)
        {
            case ValidationException validationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Validation Failure";
                problemDetails.Detail = "One or more validation errors occurred.";
                problemDetails.Extensions["errors"] = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                break;

            case ShipmentNotFoundException or WarehouseNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                problemDetails.Status = (int)HttpStatusCode.NotFound;
                problemDetails.Title = "Resource Not Found";
                problemDetails.Detail = exception.Message;
                break;

            case InvalidShipmentTransitionException:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                problemDetails.Status = (int)HttpStatusCode.Conflict;
                problemDetails.Title = "Invalid State Transition";
                problemDetails.Detail = exception.Message;
                break;

            case DomainException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Domain Business Rule Violation";
                problemDetails.Detail = exception.Message;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                problemDetails.Title = "Internal Server Error";
                problemDetails.Detail = "An unexpected error occurred. Please contact the logistics support team.";
                break;
        }

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }
}
