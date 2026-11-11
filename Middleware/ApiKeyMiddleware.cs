using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace CLTTestApi.Middleware;

/// <summary>
/// Valida el header X-API-KEY contra el valor configurado en appsettings.json ("ApiKey").
/// Si falta o no coincide, responde 401 Unauthorized.
/// </summary>
public sealed class ApiKeyMiddleware(
    RequestDelegate next,
    IConfiguration configuration,
    ILogger<ApiKeyMiddleware> logger)
{
    public const string HeaderName = "X-API-KEY";

    private readonly string? _apiKey = configuration["ApiKey"];

    public async Task InvokeAsync(HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            logger.LogError("No se encontró la configuración 'ApiKey' en appsettings.json.");
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Error de configuración",
                "La API Key del servidor no está configurada.");
            return;
        }

        var providedKey = context.Request.Headers[HeaderName].ToString();

        if (string.IsNullOrEmpty(providedKey) || !SecureEquals(_apiKey, providedKey))
        {
            logger.LogWarning(
                "Solicitud rechazada en {Path}: header {Header} ausente o inválido.",
                context.Request.Path,
                HeaderName);

            await WriteProblemAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                $"Debe enviar un header '{HeaderName}' válido.");
            return;
        }

        await next(context);
    }

    private static bool SecureEquals(string expected, string provided)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var providedBytes = Encoding.UTF8.GetBytes(provided);

        return expectedBytes.Length == providedBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            },
            options: null,
            contentType: "application/problem+json");
    }
}
