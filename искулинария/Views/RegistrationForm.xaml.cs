using System.Windows;
using искулинария.DbService;

namespace искулинария.Views;

public partial class RegistrationForm : Window
{
    public RegistrationForm()
    {
        InitializeComponent();
        Loaded += (_, _) => LoginBox.Focus();
    }

    private void RegisterButton_OnClick(object sender, RoutedEventArgs e)
    {
        var login = LoginBox.Text.Trim();
        var firstName = FirstNameBox.Text.Trim();
        var lastName = LastNameBox.Text.Trim();
        var password = PasswordBox.Password;
        var repeatedPassword = RepeatPasswordBox.Password;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(repeatedPassword))
        {
            StatusText.Text = "Заполните все поля.";
            return;
        }

        if (!UserFromDb.IsPasswordValid(password))
        {
            StatusText.Text = "Пароль должен содержать не менее 6 символов, заглавную букву, цифру и один символ из ! @ # $ % ^.";
            return;
        }

        if (password != repeatedPassword)
        {
            StatusText.Text = "Пароли не совпадают.";
            return;
        }

        try
        {
            RegisterButton.IsEnabled = false;
            StatusText.Text = "Создание учетной записи…";

            using var dataSource = DbConnect.CreateDataSource();
            var users = new UserFromDb(dataSource);

            if (users.UserExists(login))
            {
                StatusText.Text = "Пользователь с таким логином уже существует.";
                return;
            }

            users.UserAdd(login, password, firstName, lastName);
            MessageBox.Show(this, "Регистрация завершена. Теперь войдите в систему.", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Information);

            var authWindow = new AuthWindow();
            Application.Current.MainWindow = authWindow;
            authWindow.Show();
            Close();
        }
        catch (Exception ex)
        {
            StatusText.Text = "Не удалось зарегистрировать пользователя.";
            MessageBox.Show(this, ex.Message, "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            RegisterButton.IsEnabled = true;
        }
    }

    private void BackButton_OnClick(object sender, RoutedEventArgs e)
    {
        var authWindow = new AuthWindow();
        Application.Current.MainWindow = authWindow;
        authWindow.Show();
        Close();
    }
}
