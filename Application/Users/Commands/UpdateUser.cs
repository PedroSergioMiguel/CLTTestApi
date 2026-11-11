using CLTTestApi.Application.Common;
using CLTTestApi.Infrastructure;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CLTTestApi.Application.Users.Commands;

public sealed record UpdateUserCommand(
    int Id,
    string Name,
    string Email,
    bool? IsActive = null);

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator(AppDbContext db)
    {
        RuleFor(command => command.Id)
            .GreaterThan(0).WithMessage("El id del usuario debe ser mayor a 0.");

        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede superar los 150 caracteres.");

        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .MaximumLength(255).WithMessage("El email no puede superar los 255 caracteres.")
            .EmailAddress().WithMessage("El formato del email no es válido.");

        RuleFor(command => command.Email)
            .MustAsync(async (command, email, cancellationToken) =>
                !await db.Users.AnyAsync(
                    user => user.Id != command.Id && user.Email == email.Trim().ToLowerInvariant(),
                    cancellationToken))
            .WithMessage("El email ya está registrado por otro usuario.")
            .When(command => !string.IsNullOrWhiteSpace(command.Email));
    }
}

public sealed class UpdateUserHandler(AppDbContext db)
{
    public async Task<UserResponse> HandleAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException($"No existe un usuario con id {command.Id}.");

        var email = command.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(
                u => u.Id != user.Id && u.Email == email,
                cancellationToken))
        {
            throw new ConflictException($"Ya existe un usuario con el email '{email}'.");
        }

        user.Name = command.Name.Trim();
        user.Email = email;
        user.IsActive = command.IsActive ?? user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return UserResponse.FromEntity(user);
    }
}
