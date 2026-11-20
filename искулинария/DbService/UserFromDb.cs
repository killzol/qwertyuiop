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

    public User? Authenticate(string login, string password)
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

        using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("login", login.Trim());
        using var reader = command.ExecuteReader();
        if (!reader.Read())
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
    public List<User> LoadAllUsers()
    {
        const string sql = """
            SELECT u.user_id, u.first_name, u.last_name, u.patronymic,
                   u.date_of_birthday, u.login, u.phone, u.adress,
                   u.role_id, r.role_name
            FROM public.users AS u
            JOIN public.roles AS r ON r.role_id = u.role_id
            """;

        var users = new List<User>();
        using var command = dataSource.CreateCommand(sql);
        using var reader = command.ExecuteReader();
        while (reader.Read())
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

    public List<KeyValuePair<int, string>> LoadRoles()
    {
        const string sql = "SELECT role_id, role_name FROM public.roles ORDER BY role_id;";
        var roles = new List<KeyValuePair<int, string>>();
        using var command = dataSource.CreateCommand(sql);
        using var reader = command.ExecuteReader();
        while (reader.Read())
            roles.Add(new KeyValuePair<int, string>(reader.GetInt32(0), reader.GetString(1)));
        return roles;
    }

    public void UpdateUserRole(int userId, int roleId)
    {
        const string sql = "UPDATE public.users SET role_id = @role_id WHERE user_id = @user_id;";
        using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("role_id", roleId);
        if (command.ExecuteNonQuery() != 1)
            throw new InvalidOperationException("Пользователь не найден.");
    }

    public void DeleteUser(int userId)
    {
        const string sql = "DELETE FROM public.users WHERE user_id = @user_id;";
        using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("user_id", userId);
        if (command.ExecuteNonQuery() != 1)
            throw new InvalidOperationException("Пользователь не найден.");
    }

    public void ResetUserPassword(int userId, string newPassword)
    {
        if (!IsPasswordValid(newPassword))
            throw new ArgumentException("Пароль не соответствует требованиям.", nameof(newPassword));

        const string sql = "UPDATE public.users SET password = @password WHERE user_id = @user_id;";
        using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("password", Verification.GetSHA512Hash(newPassword));
        if (command.ExecuteNonQuery() != 1)
            throw new InvalidOperationException("Пользователь не найден.");
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
