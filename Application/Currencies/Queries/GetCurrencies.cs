using CLTTestApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CLTTestApi.Application.Currencies.Queries;

public sealed record GetCurrenciesQuery();

public sealed class GetCurrenciesHandler(AppDbContext db)
{
    public async Task<IReadOnlyList<CurrencyResponse>> HandleAsync(
        GetCurrenciesQuery query,
        CancellationToken cancellationToken = default)
    {
        return await db.Currencies
            .AsNoTracking()
            .OrderBy(currency => currency.Code)
            .Select(currency => new CurrencyResponse(
                currency.Id,
                currency.Code,
                currency.Name,
                currency.RateToBase))
            .ToListAsync(cancellationToken);
    }
}
