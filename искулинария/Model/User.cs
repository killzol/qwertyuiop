namespace искулинария.Model;

public sealed class User
{
    public int UserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Patronymic { get; init; } = string.Empty;
    public DateTime? DateOfBirthday { get; init; }
    public string Login { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public int RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;

    public string FullName => string.Join(" ", new[] { LastName, FirstName, Patronymic }.Where(x => !string.IsNullOrWhiteSpace(x)));
}

public static class UserRoles
{
    public const int Administrator = 1;
    public const int Manager = 2;
    public const int Chef = 3;
}
