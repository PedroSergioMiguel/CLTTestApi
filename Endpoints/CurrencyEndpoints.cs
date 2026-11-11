using CLTTestApi.Application.Common;
using CLTTestApi.Application.Currencies;
using CLTTestApi.Application.Currencies.Commands;
using CLTTestApi.Application.Currencies.Queries;
using CLTTestApi.Application.CurrencyConversion;
using FluentValidation;

namespace CLTTestApi.Endpoints;

public static class CurrencyEndpoints
{
    public static IEndpointRouteBuilder MapCurrencyEndpoints(this IEndpointRouteBuilder app)
    {
        var currencies = app.MapGroup("/currencies").WithTags("Currencies");

        // GET /currencies
        currencies.MapGet("", async (
            GetCurrenciesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetCurrenciesQuery(), cancellationToken);

            return Results.Ok(result);
        })
        .WithName("GetCurrencies")
        .Produces<IReadOnlyList<CurrencyResponse>>();

        // POST /currencies
        currencies.MapPost("", async (
            CreateCurrencyCommand command,
            IValidator<CreateCurrencyCommand> validator,
            CreateCurrencyHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (await validator.ValidateToDictionaryAsync(command, cancellationToken) is { } errors)
            {
                return Results.ValidationProblem(errors);
            }

            var currency = await handler.HandleAsync(command, cancellationToken);

            return Results.Created($"/currencies/{currency.Id}", currency);
        })
        .WithName("CreateCurrency")
        .Produces<CurrencyResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status409Conflict);

        // POST /currency/convert
        app.MapPost("/currency/convert", async (
            ConvertCurrencyCommand command,
            IValidator<ConvertCurrencyCommand> validator,
            ConvertCurrencyHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (await validator.ValidateToDictionaryAsync(command, cancellationToken) is { } errors)
            {
                return Results.ValidationProblem(errors);
            }

            var result = await handler.HandleAsync(command, cancellationToken);

            return Results.Ok(result);
        })
        .WithTags("CurrencyConversion")
        .WithName("ConvertCurrency")
        .Produces<ConvertCurrencyResponse>()
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
