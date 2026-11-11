using CLTTestApi.Application.Common;
using CLTTestApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CLTTestApi.Application.Users.Queries;

public sealed record GetUserByIdQuery(int Id);

public sealed class GetUserByIdHandler(AppDbContext db)
{
    public async Task<UserResponse> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == query.Id)
            .Select(u => new UserResponse(
                u.Id,
                u.Name,
                u.Email,
                u.IsActive,
                u.CreatedAt,
                u.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"No existe un usuario con id {query.Id}.");

        return user;
    }
}
