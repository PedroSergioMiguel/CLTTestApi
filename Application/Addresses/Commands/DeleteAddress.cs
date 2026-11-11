using CLTTestApi.Application.Common;
using CLTTestApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CLTTestApi.Application.Addresses.Commands;

public sealed record DeleteAddressCommand(int Id);

public sealed class DeleteAddressHandler(AppDbContext db)
{
    public async Task HandleAsync(DeleteAddressCommand command, CancellationToken cancellationToken = default)
    {
        var address = await db.Addresses
            .FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException($"No existe una dirección con id {command.Id}.");

        db.Addresses.Remove(address);
        await db.SaveChangesAsync(cancellationToken);
    }
}
