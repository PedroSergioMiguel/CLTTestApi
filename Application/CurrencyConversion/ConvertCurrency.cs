using CLTTestApi.Application.Common;
using CLTTestApi.Infrastructure;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CLTTestApi.Application.CurrencyConversion;

/// <summary>
/// Comando de conversión. Fórmula:
/// montoBase = amount * from.RateToBase
/// convertedAmount = montoBase / to.RateToBase
/// </summary>
public sealed record ConvertCurrencyCommand(
    string FromCurrencyCode,
    string ToCurrencyCode,
    decimal Amount);

public sealed record ConvertCurrencyResponse(
    string FromCurrency,
    string ToCurrency,
    decimal OriginalAmount,
    decimal ConvertedAmount);

public sealed class ConvertCurrencyCommandValidator : AbstractValidator<ConvertCurrencyCommand>
{
    public ConvertCurrencyCommandValidator()
    {
        RuleFor(command => command.FromCurrencyCode)
            .NotEmpty().WithMessage("La moneda de origen es obligatoria.")
            .MaximumLength(10).WithMessage("El código de moneda no puede superar los 10 caracteres.");

        RuleFor(command => command.ToCurrencyCode)
            .NotEmpty().WithMessage("La moneda de destino es obligatoria.")
            .MaximumLength(10).WithMessage("El código de moneda no puede superar los 10 caracteres.");

        RuleFor(command => command.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a 0.");
    }
}

public sealed class ConvertCurrencyHandler(AppDbContext db)
{
    public async Task<ConvertCurrencyResponse> HandleAsync(
        ConvertCurrencyCommand command,
        CancellationToken cancellationToken = default)
    {
        var fromCode = command.FromCurrencyCode.Trim().ToUpperInvariant();
        var toCode = command.ToCurrencyCode.Trim().ToUpperInvariant();

        var currencies = await db.Currencies
            .AsNoTracking()
            .Where(currency => currency.Code == fromCode || currency.Code == toCode)
            .ToListAsync(cancellationToken);

        var fromCurrency = currencies.FirstOrDefault(currency => currency.Code == fromCode)
            ?? throw new NotFoundException($"No existe la moneda '{fromCode}'.");

        var toCurrency = currencies.FirstOrDefault(currency => currency.Code == toCode)
            ?? throw new NotFoundException($"No existe la moneda '{toCode}'.");

        var amountInBase = command.Amount * fromCurrency.RateToBase;

        if (toCurrency.RateToBase == 0)
        {
            throw new ConflictException($"La moneda '{toCode}' tiene un tipo de cambio inválido (0).");
        }

        var convertedAmount = decimal.Round(
            amountInBase / toCurrency.RateToBase,
            4,
            MidpointRounding.AwayFromZero);

        return new ConvertCurrencyResponse(
            fromCurrency.Code,
            toCurrency.Code,
            command.Amount,
            convertedAmount);
    }
}
