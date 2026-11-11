using CLTTestApi.Domain;

namespace CLTTestApi.Application.Users;

public sealed record UserResponse(
    int Id,
    string Name,
    string Email,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static UserResponse FromEntity(User user) =>
        new(user.Id, user.Name, user.Email, user.IsActive, user.CreatedAt, user.UpdatedAt);
}
