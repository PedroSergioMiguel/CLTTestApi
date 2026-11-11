using CLTTestApi.Domain;

namespace CLTTestApi.Application.Currencies;

public sealed record CurrencyResponse(
    int Id,
    string Code,
    string Name,
    decimal RateToBase)
{
    public static CurrencyResponse FromEntity(Currency currency) =>
        new(currency.Id, currency.Code, currency.Name, currency.RateToBase);
}
