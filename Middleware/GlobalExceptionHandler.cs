using CLTTestApi.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLTTestApi.Middleware;

/// <summary>
/// Traduce las excepciones de aplicación a respuestas ProblemDetails
/// (404 para NotFoundException, 409 para ConflictException, 500 para el resto).
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Not Found",
                exception.Message),

            ConflictException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                exception.Message),

            BadHttpRequestException => (
                StatusCodes.Status400BadRequest,
                "Bad Request",
                "El cuerpo o los parámetros de la solicitud no son válidos."),

            DbUpdateException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                "La operación viola una restricción de unicidad o integridad de la base de datos."),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "Ocurrió un error inesperado al procesar la solicitud.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Error no controlado procesando {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(
                "La solicitud {Method} {Path} finalizó con estado {StatusCode}: {Message}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                statusCode,
                exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            },
            options: null,
            contentType: "application/problem+json",
            cancellationToken);

        return true;
    }
}
