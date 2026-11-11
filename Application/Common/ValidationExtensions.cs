using FluentValidation;

namespace CLTTestApi.Application.Common;

public static class ValidationExtensions
{
    /// <summary>
    /// Ejecuta un validador y devuelve el diccionario de errores, o <c>null</c> si la instancia es válida.
    /// </summary>
    public static async Task<IDictionary<string, string[]>?> ValidateToDictionaryAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);

        return result.IsValid ? null : result.ToDictionary();
    }
}
