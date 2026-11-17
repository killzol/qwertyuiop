using искулинария.Model;
using Npgsql;

namespace искулинария.DbService;

public sealed class UserFromDb(NpgsqlDataSource dataSource)
{
    public async Task<User?> AuthenticateAsync(string login, string password, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT u.user_id, u.first_name, u.last_name, u.patronymic,
                   u.date_of_birthday, u.login, u.password, u.phone, u.adress,
                   u.role_id, r.role_name
            FROM public.users AS u
            JOIN public.roles AS r ON r.role_id = u.role_id
            WHERE LOWER(u.login) = LOWER(@login)
            LIMIT 1;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("login", login.Trim());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var passwordHash = reader.GetString(6);
        if (!Verification.VerifySHA512Hash(password, passwordHash))
            return null;

        return ReadUser(reader);
    }

    public async Task<List<User>> LoadAllUsersAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT u.user_id, u.first_name, u.last_name, u.patronymic,
                   u.date_of_birthday, u.login, u.phone, u.adress,
                   u.role_id, r.role_name
            FROM public.users AS u
            JOIN public.roles AS r ON r.role_id = u.role_id
            ORDER BY u.last_name, u.first_name;
            """;

        var users = new List<User>();
        await using var command = dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(new User
            {
                UserId = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Patronymic = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                DateOfBirthday = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                Login = reader.GetString(5),
                Phone = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                Address = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                RoleId = reader.GetInt32(8),
                RoleName = reader.GetString(9)
            });
        }

        return users;
    }

    private static User ReadUser(NpgsqlDataReader reader) => new()
    {
        UserId = reader.GetInt32(0),
        FirstName = reader.GetString(1),
        LastName = reader.GetString(2),
        Patronymic = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
        DateOfBirthday = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
        Login = reader.GetString(5),
        Phone = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
        Address = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
        RoleId = reader.GetInt32(9),
        RoleName = reader.GetString(10)
    };
}
