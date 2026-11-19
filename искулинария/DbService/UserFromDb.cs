using искулинария.Model;
using Npgsql;

namespace искулинария.DbService;

public sealed class UserFromDb(NpgsqlDataSource dataSource)
{
    public static bool IsPasswordValid(string password) =>
        password.Length >= 6 &&
        password.Any(char.IsUpper) &&
        password.Any(char.IsDigit) &&
        password.Any(character => "!@#$%^".Contains(character));

    public bool UserExists(string login)
    {
        const string sql = "SELECT EXISTS (SELECT 1 FROM public.users WHERE LOWER(login) = LOWER(@login));";
        using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("login", login.Trim());
        return (bool)command.ExecuteScalar()!;
    }

    public void UserAdd(string login, string password, string firstName, string lastName)
    {
        const string sql = "INSERT INTO public.users (login, password, first_name, last_name) VALUES (@login, @password, @first_name, @last_name);";
        using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("login", login.Trim());
        command.Parameters.AddWithValue("password", Verification.GetSHA512Hash(password));
        command.Parameters.AddWithValue("first_name", firstName.Trim());
        command.Parameters.AddWithValue("last_name", lastName.Trim());
        command.ExecuteNonQuery();
    }

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

        var user = ReadUser(reader);
        user.PasswordHash = passwordHash;
        return user;
    }

    public void UserUpdateProfil(User user, bool isPasswordChanged)
    {
        var sql = isPasswordChanged
            ? """
              UPDATE public.users
              SET first_name = @first_name,
                  last_name = @last_name,
                  patronymic = @patronymic,
                  date_of_birthday = @date_of_birthday,
                  login = @login,
                  phone = @phone,
                  adress = @adress,
                  password = @password
              WHERE user_id = @user_id;
              """
            : """
              UPDATE public.users
              SET first_name = @first_name,
                  last_name = @last_name,
                  patronymic = @patronymic,
                  date_of_birthday = @date_of_birthday,
                  login = @login,
                  phone = @phone,
                  adress = @adress
              WHERE user_id = @user_id;
              """;

        if (isPasswordChanged && string.IsNullOrWhiteSpace(user.PasswordHash))
            throw new InvalidOperationException("Не задан новый хеш пароля.");

        using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("user_id", user.UserId);
        command.Parameters.AddWithValue("first_name", user.FirstName.Trim());
        command.Parameters.AddWithValue("last_name", user.LastName.Trim());
        command.Parameters.AddWithValue("patronymic", (object?)NullIfEmpty(user.Patronymic) ?? DBNull.Value);
        command.Parameters.AddWithValue("date_of_birthday", NpgsqlTypes.NpgsqlDbType.Date, (object?)user.DateOfBirthday?.Date ?? DBNull.Value);
        command.Parameters.AddWithValue("login", user.Login.Trim());
        command.Parameters.AddWithValue("phone", (object?)NullIfEmpty(user.Phone) ?? DBNull.Value);
        command.Parameters.AddWithValue("adress", (object?)NullIfEmpty(user.Address) ?? DBNull.Value);
        if (isPasswordChanged)
            command.Parameters.AddWithValue("password", user.PasswordHash);

        if (command.ExecuteNonQuery() != 1)
            throw new InvalidOperationException("Профиль пользователя не найден в базе данных.");
    }

    private static string? NullIfEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
