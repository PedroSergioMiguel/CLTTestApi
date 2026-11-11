using CLTTestApi.Application.Addresses.Commands;
using CLTTestApi.Application.Addresses.Queries;
using CLTTestApi.Application.Currencies.Commands;
using CLTTestApi.Application.Currencies.Queries;
using CLTTestApi.Application.CurrencyConversion;
using CLTTestApi.Application.Users.Commands;
using CLTTestApi.Application.Users.Queries;
using CLTTestApi.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CLTTestApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registra todos los validadores de FluentValidation del ensamblado.
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Users
        services.AddScoped<CreateUserHandler>();
        services.AddScoped<UpdateUserHandler>();
        services.AddScoped<DeleteUserHandler>();
        services.AddScoped<GetUsersHandler>();
        services.AddScoped<GetUserByIdHandler>();

        // Addresses
        services.AddScoped<CreateAddressHandler>();
        services.AddScoped<UpdateAddressHandler>();
        services.AddScoped<DeleteAddressHandler>();
        services.AddScoped<GetAddressesByUserHandler>();

        // Currencies y conversión
        services.AddScoped<CreateCurrencyHandler>();
        services.AddScoped<GetCurrenciesHandler>();
        services.AddScoped<ConvertCurrencyHandler>();

        // Hashing de contraseñas (ASP.NET Core Identity, sin almacenamiento Identity completo).
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

        return services;
    }
}
