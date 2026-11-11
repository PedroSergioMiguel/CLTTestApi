using CLTTestApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace CLTTestApi.Infrastructure;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Currencies.AnyAsync(cancellationToken))
        {
            return;
        }

        db.Currencies.AddRange(
            new Currency
            {
                Code = "PYG",
                Name = "Guaraní paraguayo",
                RateToBase = 1m
            },
            new Currency
            {
                Code = "USD",
                Name = "Dólar estadounidense",
                RateToBase = 7300m
            });

        await db.SaveChangesAsync(cancellationToken);
    }
}
