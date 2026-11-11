namespace CLTTestApi.Application.Common;

/// <summary>Se lanza cuando un recurso solicitado no existe (HTTP 404).</summary>
public sealed class NotFoundException(string message) : Exception(message);

/// <summary>Se lanza cuando una operación entra en conflicto con el estado actual (HTTP 409).</summary>
public sealed class ConflictException(string message) : Exception(message);
