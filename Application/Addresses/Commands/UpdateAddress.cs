using CLTTestApi.Application.Common;
using CLTTestApi.Infrastructure;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CLTTestApi.Application.Addresses.Commands;

public sealed record UpdateAddressCommand(
    int Id,
    string Street,
    string City,
    string Country,
    string? ZipCode = null);

public sealed class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0).WithMessage("El id de la dirección debe ser mayor a 0.");

        RuleFor(command => command.Street)
            .NotEmpty().WithMessage("La calle es obligatoria.")
            .MaximumLength(200).WithMessage("La calle no puede superar los 200 caracteres.");

        RuleFor(command => command.City)
            .NotEmpty().WithMessage("La ciudad es obligatoria.")
            .MaximumLength(100).WithMessage("La ciudad no puede superar los 100 caracteres.");

        RuleFor(command => command.Country)
            .NotEmpty().WithMessage("El país es obligatorio.")
            .MaximumLength(100).WithMessage("El país no puede superar los 100 caracteres.");

        RuleFor(command => command.ZipCode)
            .MaximumLength(20).WithMessage("El código postal no puede superar los 20 caracteres.");
    }
}

public sealed class UpdateAddressHandler(AppDbContext db)
{
    public async Task<AddressResponse> HandleAsync(
        UpdateAddressCommand command,
        CancellationToken cancellationToken = default)
    {
        var address = await db.Addresses
            .FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException($"No existe una dirección con id {command.Id}.");

        address.Street = command.Street.Trim();
        address.City = command.City.Trim();
        address.Country = command.Country.Trim();
        address.ZipCode = string.IsNullOrWhiteSpace(command.ZipCode) ? null : command.ZipCode.Trim();

        await db.SaveChangesAsync(cancellationToken);

        return AddressResponse.FromEntity(address);
    }
}
