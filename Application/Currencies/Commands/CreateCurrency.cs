using CLTTestApi.Application.Common;
using CLTTestApi.Domain;
using CLTTestApi.Infrastructure;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CLTTestApi.Application.Currencies.Commands;

public sealed record CreateCurrencyCommand(
    string Code,
    string Name,
    decimal RateToBase);

public sealed class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyCommandValidator(AppDbContext db)
    {
        RuleFor(command => command.Code)
            .NotEmpty().WithMessage("El código de la moneda es obligatorio.")
            .MaximumLength(10).WithMessage("El código no puede superar los 10 caracteres.");

        RuleFor(command => command.Code)
            .MustAsync(async (code, cancellationToken) =>
                !await db.Currencies.AnyAsync(
                    currency => currency.Code == code.Trim().ToUpperInvariant(),
                    cancellationToken))
            .WithMessage("El código de moneda ya existe.")
            .When(command => !string.IsNullOrWhiteSpace(command.Code));

        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("El nombre de la moneda es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(command => command.RateToBase)
            .GreaterThan(0).WithMessage("El tipo de cambio debe ser mayor a 0.");
    }
}

public sealed class CreateCurrencyHandler(AppDbContext db)
{
    public async Task<CurrencyResponse> HandleAsync(
        CreateCurrencyCommand command,
        CancellationToken cancellationToken = default)
    {
        var code = command.Code.Trim().ToUpperInvariant();

        if (await db.Currencies.AnyAsync(currency => currency.Code == code, cancellationToken))
        {
            throw new ConflictException($"Ya existe una moneda con el código '{code}'.");
        }

        var currency = new Currency
        {
            Code = code,
            Name = command.Name.Trim(),
            RateToBase = command.RateToBase
        };

        db.Currencies.Add(currency);
        await db.SaveChangesAsync(cancellationToken);

        return CurrencyResponse.FromEntity(currency);
    }
}
