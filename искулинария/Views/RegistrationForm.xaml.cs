using System.Windows;
using искулинария.DbService;

namespace искулинария.Views;

public partial class RegistrationForm : Window
{
    private readonly bool _openedFromAdmin;

    public RegistrationForm(bool openedFromAdmin = false)
    {
        InitializeComponent();
        _openedFromAdmin = openedFromAdmin;
        Loaded += RegistrationForm_OnLoaded;
    }

    private void RegistrationForm_OnLoaded(object sender, RoutedEventArgs e)
    {
        LoginBox.Focus();
    }

    private void RegisterButton_OnClick(object sender, RoutedEventArgs e)
    {
        var login = LoginBox.Text.Trim();
        var imya = FirstNameBox.Text.Trim();
        var familiya = LastNameBox.Text.Trim();
        var parol = PasswordBox.Password;
        var povtorParol = RepeatPasswordBox.Password;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(imya) ||
            string.IsNullOrWhiteSpace(familiya) || string.IsNullOrWhiteSpace(parol) ||
            string.IsNullOrWhiteSpace(povtorParol))
        {
            StatusText.Text = "Заполните все поля.";
            return;
        }

        if (!UserFromDb.IsPasswordValid(parol))
        {
            StatusText.Text = "Пароль должен содержать не менее 6 символов, заглавную букву, цифру и один символ из ! @ # $ % ^.";
            return;
        }

        if (parol != povtorParol)
        {
            StatusText.Text = "Пароли не совпадают.";
            return;
        }

        try
        {
            using var podklyuchenie = DbConnect.CreateDataSource();
            var polzovateli = new UserFromDb(podklyuchenie);
            if (polzovateli.UserExists(login))
            {
                StatusText.Text = "Пользователь с таким логином уже существует.";
                return;
            }

            polzovateli.UserAdd(login, parol, imya, familiya);
            if (_openedFromAdmin)
            {
                DialogResult = true;
                Close();
                return;
            }

            MessageBox.Show(this, "Регистрация успешна.", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }

    private void BackButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_openedFromAdmin)
        {
            DialogResult = false;
            Close();
            return;
        }

        var authWindow = new AuthWindow();
        Application.Current.MainWindow = authWindow;
        authWindow.Show();
        Close();
    }
}
