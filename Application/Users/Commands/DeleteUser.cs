using CLTTestApi.Application.Common;
using CLTTestApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CLTTestApi.Application.Users.Commands;

public sealed record DeleteUserCommand(int Id);

public sealed class DeleteUserHandler(AppDbContext db)
{
    public async Task HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException($"No existe un usuario con id {command.Id}.");

        // Las direcciones se eliminan en cascada (configurado en AppDbContext).
        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);
    }
}
