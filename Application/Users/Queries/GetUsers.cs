using CLTTestApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CLTTestApi.Application.Users.Queries;

public sealed record GetUsersQuery(bool? IsActive = null);

public sealed class GetUsersHandler(AppDbContext db)
{
    public async Task<IReadOnlyList<UserResponse>> HandleAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        var users = db.Users.AsNoTracking();

        if (query.IsActive.HasValue)
        {
            users = users.Where(user => user.IsActive == query.IsActive.Value);
        }

        return await users
            .OrderBy(user => user.Id)
            .Select(user => new UserResponse(
                user.Id,
                user.Name,
                user.Email,
                user.IsActive,
                user.CreatedAt,
                user.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
