using CLTTestApi.Application.Common;
using CLTTestApi.Application.Users;
using CLTTestApi.Application.Users.Commands;
using CLTTestApi.Application.Users.Queries;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CLTTestApi.Endpoints;

public sealed record CreateUserRequest(
    string Name,
    string Email,
    string? Password = null,
    bool? IsActive = null);

public sealed record UpdateUserRequest(
    string Name,
    string Email,
    bool? IsActive = null);

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users").WithTags("Users");

        // POST /users
        group.MapPost("", async (
            CreateUserRequest request,
            IValidator<CreateUserCommand> validator,
            CreateUserHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateUserCommand(
                request.Name,
                request.Email,
                request.Password,
                request.IsActive);

            if (await validator.ValidateToDictionaryAsync(command, cancellationToken) is { } errors)
            {
                return Results.ValidationProblem(errors);
            }

            var user = await handler.HandleAsync(command, cancellationToken);

            return Results.Created($"/users/{user.Id}", user);
        })
        .WithName("CreateUser")
        .Produces<UserResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status409Conflict);

        // GET /users?isActive=true|false (también acepta is_active)
        group.MapGet("", async (
            bool? isActive,
            [FromQuery(Name = "is_active")] bool? isActiveSnakeCase,
            GetUsersHandler handler,
            CancellationToken cancellationToken) =>
        {
            var filter = isActive ?? isActiveSnakeCase;
            var users = await handler.HandleAsync(new GetUsersQuery(filter), cancellationToken);

            return Results.Ok(users);
        })
        .WithName("GetUsers")
        .Produces<IReadOnlyList<UserResponse>>();

        // GET /users/{id}
        group.MapGet("/{id:int}", async (
            int id,
            GetUserByIdHandler handler,
            CancellationToken cancellationToken) =>
        {
            var user = await handler.HandleAsync(new GetUserByIdQuery(id), cancellationToken);

            return Results.Ok(user);
        })
        .WithName("GetUserById")
        .Produces<UserResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        // PUT /users/{id}
        group.MapPut("/{id:int}", async (
            int id,
            UpdateUserRequest request,
            IValidator<UpdateUserCommand> validator,
            UpdateUserHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateUserCommand(
                id,
                request.Name,
                request.Email,
                request.IsActive);

            if (await validator.ValidateToDictionaryAsync(command, cancellationToken) is { } errors)
            {
                return Results.ValidationProblem(errors);
            }

            var user = await handler.HandleAsync(command, cancellationToken);

            return Results.Ok(user);
        })
        .WithName("UpdateUser")
        .Produces<UserResponse>()
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        // DELETE /users/{id}
        group.MapDelete("/{id:int}", async (
            int id,
            DeleteUserHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(new DeleteUserCommand(id), cancellationToken);

            return Results.NoContent();
        })
        .WithName("DeleteUser")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
