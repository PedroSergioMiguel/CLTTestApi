using CLTTestApi.Application.Addresses;
using CLTTestApi.Application.Addresses.Commands;
using CLTTestApi.Application.Addresses.Queries;
using CLTTestApi.Application.Common;
using FluentValidation;

namespace CLTTestApi.Endpoints;

public sealed record CreateAddressRequest(
    string Street,
    string City,
    string Country,
    string? ZipCode = null);

public sealed record UpdateAddressRequest(
    string Street,
    string City,
    string Country,
    string? ZipCode = null);

public static class AddressEndpoints
{
    public static IEndpointRouteBuilder MapAddressEndpoints(this IEndpointRouteBuilder app)
    {
        var userAddresses = app.MapGroup("/users/{userId:int}/addresses").WithTags("Addresses");

        // POST /users/{userId}/addresses
        userAddresses.MapPost("", async (
            int userId,
            CreateAddressRequest request,
            IValidator<CreateAddressCommand> validator,
            CreateAddressHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateAddressCommand(
                userId,
                request.Street,
                request.City,
                request.Country,
                request.ZipCode);

            if (await validator.ValidateToDictionaryAsync(command, cancellationToken) is { } errors)
            {
                return Results.ValidationProblem(errors);
            }

            var address = await handler.HandleAsync(command, cancellationToken);

            return Results.Created($"/addresses/{address.Id}", address);
        })
        .WithName("CreateAddress")
        .Produces<AddressResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound);

        // GET /users/{userId}/addresses
        userAddresses.MapGet("", async (
            int userId,
            GetAddressesByUserHandler handler,
            CancellationToken cancellationToken) =>
        {
            var addresses = await handler.HandleAsync(
                new GetAddressesByUserQuery(userId),
                cancellationToken);

            return Results.Ok(addresses);
        })
        .WithName("GetAddressesByUser")
        .Produces<IReadOnlyList<AddressResponse>>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        var addresses = app.MapGroup("/addresses").WithTags("Addresses");

        // PUT /addresses/{id}
        addresses.MapPut("/{id:int}", async (
            int id,
            UpdateAddressRequest request,
            IValidator<UpdateAddressCommand> validator,
            UpdateAddressHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateAddressCommand(
                id,
                request.Street,
                request.City,
                request.Country,
                request.ZipCode);

            if (await validator.ValidateToDictionaryAsync(command, cancellationToken) is { } errors)
            {
                return Results.ValidationProblem(errors);
            }

            var address = await handler.HandleAsync(command, cancellationToken);

            return Results.Ok(address);
        })
        .WithName("UpdateAddress")
        .Produces<AddressResponse>()
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /addresses/{id}
        addresses.MapDelete("/{id:int}", async (
            int id,
            DeleteAddressHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(new DeleteAddressCommand(id), cancellationToken);

            return Results.NoContent();
        })
        .WithName("DeleteAddress")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
