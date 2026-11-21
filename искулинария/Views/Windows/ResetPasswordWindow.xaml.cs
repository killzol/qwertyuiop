using System.Windows;
using искулинария.DbService;
using искулинария.Model;

namespace искулинария.Views.Windows;

public partial class ResetPasswordWindow : Window
{
    private readonly User _polzovatel;

    public ResetPasswordWindow(User polzovatel)
    {
        InitializeComponent();
        _polzovatel = polzovatel;
        UserNameText.Text = polzovatel.FullName;
    }

    private void ChangeButton_OnClick(object sender, RoutedEventArgs e)
    {
        StatusText.Text = string.Empty;
        var parol = NewPasswordBox.Password;
        var podtverzhdenie = ConfirmPasswordBox.Password;

        if (string.IsNullOrWhiteSpace(parol) || string.IsNullOrWhiteSpace(podtverzhdenie))
        {
            StatusText.Text = "Заполните оба поля пароля.";
            return;
        }

        if (!UserFromDb.IsPasswordValid(parol))
        {
            StatusText.Text = "Пароль должен содержать не менее 6 символов, заглавную букву, цифру и один символ из ! @ # $ % ^.";
            return;
        }

        if (parol != podtverzhdenie)
        {
            StatusText.Text = "Пароли не совпадают.";
            return;
        }

        var answer = MessageBox.Show(
            this,
            $"Сейчас будет изменён пароль пользователя {_polzovatel.FullName}. Продолжить?",
            "Подтверждение сброса пароля",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning);
        if (answer != MessageBoxResult.OK)
            return;

        try
        {
            using var podklyuchenie = DbConnect.CreateDataSource();
            new UserFromDb(podklyuchenie).ResetUserPassword(_polzovatel.UserId, parol);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            StatusText.Text = "Не удалось изменить пароль.";
            MessageBox.Show(this, ex.Message, "Ошибка сброса пароля", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
