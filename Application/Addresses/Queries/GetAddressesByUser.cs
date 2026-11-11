using CLTTestApi.Application.Common;
using CLTTestApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CLTTestApi.Application.Addresses.Queries;

public sealed record GetAddressesByUserQuery(int UserId);

public sealed class GetAddressesByUserHandler(AppDbContext db)
{
    public async Task<IReadOnlyList<AddressResponse>> HandleAsync(
        GetAddressesByUserQuery query,
        CancellationToken cancellationToken = default)
    {
        var userExists = await db.Users
            .AnyAsync(user => user.Id == query.UserId, cancellationToken);

        if (!userExists)
        {
            throw new NotFoundException($"No existe un usuario con id {query.UserId}.");
        }

        return await db.Addresses
            .AsNoTracking()
            .Where(address => address.UserId == query.UserId)
            .OrderBy(address => address.Id)
            .Select(address => new AddressResponse(
                address.Id,
                address.UserId,
                address.Street,
                address.City,
                address.Country,
                address.ZipCode))
            .ToListAsync(cancellationToken);
    }
}
