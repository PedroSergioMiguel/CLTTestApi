using CLTTestApi.Application.Common;
using CLTTestApi.Domain;
using CLTTestApi.Infrastructure;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CLTTestApi.Application.Addresses.Commands;

public sealed record CreateAddressCommand(
    int UserId,
    string Street,
    string City,
    string Country,
    string? ZipCode = null);

public sealed class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
{
    public CreateAddressCommandValidator()
    {
        RuleFor(command => command.UserId)
            .GreaterThan(0).WithMessage("El id del usuario debe ser mayor a 0.");

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

public sealed class CreateAddressHandler(AppDbContext db)
{
    public async Task<AddressResponse> HandleAsync(
        CreateAddressCommand command,
        CancellationToken cancellationToken = default)
    {
        var userExists = await db.Users
            .AnyAsync(user => user.Id == command.UserId, cancellationToken);

        if (!userExists)
        {
            throw new NotFoundException($"No existe un usuario con id {command.UserId}.");
        }

        var address = new Address
        {
            UserId = command.UserId,
            Street = command.Street.Trim(),
            City = command.City.Trim(),
            Country = command.Country.Trim(),
            ZipCode = string.IsNullOrWhiteSpace(command.ZipCode) ? null : command.ZipCode.Trim()
        };

        db.Addresses.Add(address);
        await db.SaveChangesAsync(cancellationToken);

        return AddressResponse.FromEntity(address);
    }
}
