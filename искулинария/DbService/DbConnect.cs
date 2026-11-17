using System.IO;
using System.Text.Json;
using Npgsql;

namespace искулинария.DbService;

public static class DbConnect
{
    public static NpgsqlDataSource CreateDataSource()
    {
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(settingsPath))
            throw new InvalidOperationException($"Не найден файл настроек: {settingsPath}");

        using var document = JsonDocument.Parse(File.ReadAllText(settingsPath));
        var connectionString = document.RootElement
            .GetProperty("ConnectionStrings")
            .GetProperty("Default")
            .GetString();

        if (string.IsNullOrWhiteSpace(connectionString) ||
            connectionString.Contains("ВАША_ФАМИЛИЯ", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("ВПИШИТЕ_ПАРОЛЬ", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Откройте appsettings.json и укажите настоящее имя базы, пользователя PostgreSQL и пароль.");
        }

        return NpgsqlDataSource.Create(connectionString);
    }
}
