using CLTTestApi.Application.Common;
using CLTTestApi.Domain;
using CLTTestApi.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CLTTestApi.Application.Users.Commands;

public sealed record CreateUserCommand(
    string Name,
    string Email,
    string? Password = null,
    bool? IsActive = null);

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(AppDbContext db)
    {
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede superar los 150 caracteres.");

        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .MaximumLength(255).WithMessage("El email no puede superar los 255 caracteres.")
            .EmailAddress().WithMessage("El formato del email no es válido.");

        RuleFor(command => command.Email)
            .MustAsync(async (email, cancellationToken) =>
                !await db.Users.AnyAsync(
                    user => user.Email == email.Trim().ToLowerInvariant(),
                    cancellationToken))
            .WithMessage("El email ya está registrado.")
            .When(command => !string.IsNullOrWhiteSpace(command.Email));

        RuleFor(command => command.Password)
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.")
            .When(command => !string.IsNullOrWhiteSpace(command.Password));
    }
}

public sealed class CreateUserHandler(AppDbContext db, IPasswordHasher<User> passwordHasher)
{
    public async Task<UserResponse> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = command.Email.Trim().ToLowerInvariant();

        // Defensa adicional ante condiciones de carrera: el índice único de la BD también lo impide.
        if (await db.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            throw new ConflictException($"Ya existe un usuario con el email '{email}'.");
        }

        var user = new User
        {
            Name = command.Name.Trim(),
            Email = email,
            IsActive = command.IsActive ?? true
        };

        if (!string.IsNullOrWhiteSpace(command.Password))
        {
            user.PasswordHash = passwordHasher.HashPassword(user, command.Password);
        }

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return UserResponse.FromEntity(user);
    }
}
