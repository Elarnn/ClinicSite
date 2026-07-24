using System.Net;
using System.Text.Json;
using ClinicSite.Application.Exceptions;

namespace ClinicSite.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
            await WriteResponseAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteResponseAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (GoneException ex)
        {
            await WriteResponseAsync(context, HttpStatusCode.Gone, ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteResponseAsync(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (EmailDeliveryException ex)
        {
            // The inner cause was already logged where it happened; keep the client message generic.
            await WriteResponseAsync(context, HttpStatusCode.ServiceUnavailable, ex.Message);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            await WriteResponseAsync(
                context,
                HttpStatusCode.Conflict,
                "The record was modified by another request. Refresh the page and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");

            await WriteResponseAsync(
                context,
                HttpStatusCode.InternalServerError,
                "An unexpected server error occurred.");
        }
    }

    private static Task WriteResponseAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = JsonSerializer.Serialize(new
        {
            status = (int)statusCode,
            message
        });

        return context.Response.WriteAsync(payload);
    }
}
