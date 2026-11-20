namespace искулинария.Model;

public sealed class User
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Patronymic { get; set; } = string.Empty;
    public DateTime? DateOfBirthday { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    internal string PasswordHash { get; set; } = string.Empty;

    public string FullName => string.Join(" ", new[] { LastName, FirstName, Patronymic }.Where(x => !string.IsNullOrWhiteSpace(x)));
}

public static class UserRoles
{
    public const int Administrator = 1;
    public const int Manager = 2;
    public const int Chef = 3;
}