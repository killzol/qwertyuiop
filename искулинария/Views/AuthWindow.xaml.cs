using System.Windows;
using System.Windows.Input;
using искулинария.DbService;
using искулинария.Model;

namespace искулинария.Views;

public partial class AuthWindow : Window
{
    public static User? CurrentUser { get; set; }

    public AuthWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => LoginBox.Focus();
    }

    private async void LoginButton_OnClick(object sender, RoutedEventArgs e)
    {
        var login = LoginBox.Text.Trim();
        var password = PasswordInput.Password;
        if (login.Length == 0 || password.Length == 0)
        {
            StatusText.Text = "Введите логин и пароль.";
            return;
        }

        LoginButton.IsEnabled = false;
        StatusText.Text = "Проверка данных…";
        try
        {
            await using var dataSource = DbConnect.CreateDataSource();
            var users = new UserFromDb(dataSource);
            var user = await users.AuthenticateAsync(login, password);
            if (user is null)
            {
                StatusText.Text = "Неверный логин или пароль.";
                return;
            }

            CurrentUser = user;
            var mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            Close();
        }
        catch (Exception ex)
        {
            StatusText.Text = "Не удалось подключиться к базе данных.";
            MessageBox.Show(this, ex.Message, "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            LoginButton.IsEnabled = true;
        }
    }

    private void RegisterButton_OnClick(object sender, RoutedEventArgs e)
    {
        var registrationWindow = new RegistrationForm();
        Application.Current.MainWindow = registrationWindow;
        registrationWindow.Show();
        Close();
    }

    private void CredentialBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            LoginButton_OnClick(sender, new RoutedEventArgs());
        }
    }
}
