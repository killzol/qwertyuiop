using System.Windows;
using искулинария.DbService;
using искулинария.Model;

namespace искулинария.Views;

public partial class AuthWindow : Window
{
    public static User? CurrentUser { get; set; }

    public AuthWindow()
    {
        InitializeComponent();
        Loaded += AuthWindow_OnLoaded;
    }

    private void AuthWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        LoginBox.Focus();
    }

    private void LoginButton_OnClick(object sender, RoutedEventArgs e)
    {
        var login = LoginBox.Text.Trim();
        var parol = PasswordBox.Password;
        if (login.Length == 0 || parol.Length == 0)
        {
            StatusText.Text = "Введите логин и пароль.";
            return;
        }

        try
        {
            using var podklyuchenie = DbConnect.CreateDataSource();
            var polzovateli = new UserFromDb(podklyuchenie);
            var polzovatel = polzovateli.Authenticate(login, parol);
            if (polzovatel is null)
            {
                StatusText.Text = "Неверный логин или пароль.";
                return;
            }

            CurrentUser = polzovatel;
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
    }

    private void RegisterButton_OnClick(object sender, RoutedEventArgs e)
    {
        var oknoRegistracii = new RegistrationForm();
        Application.Current.MainWindow = oknoRegistracii;
        oknoRegistracii.Show();
        Close();
    }
}
