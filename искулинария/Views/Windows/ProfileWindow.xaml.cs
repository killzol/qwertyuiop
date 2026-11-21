using System.Windows;
using искулинария.DbService;
using искулинария.Model;

namespace искулинария.Views.Windows;

public partial class ProfileWindow : Window
{
    private readonly User _tekushiyPolzovatel;
    private bool _parolIzmenen;

    public ProfileWindow(User tekushiyPolzovatel)
    {
        InitializeComponent();
        _tekushiyPolzovatel = tekushiyPolzovatel;
    }

    private void ProfileWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        FirstNameBox.Text = _tekushiyPolzovatel.FirstName;
        LastNameBox.Text = _tekushiyPolzovatel.LastName;
        PatronymicBox.Text = _tekushiyPolzovatel.Patronymic;
        DateOfBirthday.SelectedDate = _tekushiyPolzovatel.DateOfBirthday;
        LoginBox.Text = _tekushiyPolzovatel.Login;
        PhoneBox.Text = _tekushiyPolzovatel.Phone;
        AddressBox.Text = _tekushiyPolzovatel.Address;

        OldPasswordBox.Clear();
        NewPasswordBox.Clear();
        ConfirmPasswordBox.Clear();
    }

    private void ChangePasswordButton_OnClick(object sender, RoutedEventArgs e)
    {
        _parolIzmenen = true;
        PasswordPanel.Visibility = Visibility.Visible;
        ChangePasswordButton.Visibility = Visibility.Collapsed;
        OldPasswordBox.Clear();
        NewPasswordBox.Clear();
        ConfirmPasswordBox.Clear();
        OldPasswordBox.Focus();
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        StatusText.Text = string.Empty;
        var imya = FirstNameBox.Text.Trim();
        var familiya = LastNameBox.Text.Trim();

        if (imya.Length == 0 || familiya.Length == 0)
        {
            StatusText.Text = "Заполните имя и фамилию.";
            return;
        }

        var noviyHashParolya = _tekushiyPolzovatel.PasswordHash;
        if (_parolIzmenen)
        {
            var stariyParol = OldPasswordBox.Password;
            var noviyParol = NewPasswordBox.Password;
            var povtorParol = ConfirmPasswordBox.Password;

            if (stariyParol.Length == 0 || noviyParol.Length == 0 || povtorParol.Length == 0)
            {
                StatusText.Text = "Заполните все поля для смены пароля.";
                return;
            }

            if (!Verification.VerifySHA512Hash(stariyParol, _tekushiyPolzovatel.PasswordHash))
            {
                StatusText.Text = "Старый пароль введён неверно.";
                return;
            }

            if (!UserFromDb.IsPasswordValid(noviyParol))
            {
                StatusText.Text = "Новый пароль не соответствует требованиям.";
                return;
            }

            if (noviyParol != povtorParol)
            {
                StatusText.Text = "Новый пароль и подтверждение не совпадают.";
                return;
            }

            noviyHashParolya = Verification.GetSHA512Hash(noviyParol);
        }

        var izmenenniyPolzovatel = new User
        {
            UserId = _tekushiyPolzovatel.UserId,
            FirstName = imya,
            LastName = familiya,
            Patronymic = PatronymicBox.Text.Trim(),
            DateOfBirthday = DateOfBirthday.SelectedDate,
            Login = LoginBox.Text.Trim(),
            Phone = PhoneBox.Text.Trim(),
            Address = AddressBox.Text.Trim(),
            RoleId = _tekushiyPolzovatel.RoleId,
            RoleName = _tekushiyPolzovatel.RoleName,
            PasswordHash = noviyHashParolya
        };

        try
        {
            using var podklyuchenie = DbConnect.CreateDataSource();
            var polzovateli = new UserFromDb(podklyuchenie);
            polzovateli.UserUpdateProfil(izmenenniyPolzovatel, _parolIzmenen);

            _tekushiyPolzovatel.FirstName = izmenenniyPolzovatel.FirstName;
            _tekushiyPolzovatel.LastName = izmenenniyPolzovatel.LastName;
            _tekushiyPolzovatel.Patronymic = izmenenniyPolzovatel.Patronymic;
            _tekushiyPolzovatel.DateOfBirthday = izmenenniyPolzovatel.DateOfBirthday;
            _tekushiyPolzovatel.Login = izmenenniyPolzovatel.Login;
            _tekushiyPolzovatel.Phone = izmenenniyPolzovatel.Phone;
            _tekushiyPolzovatel.Address = izmenenniyPolzovatel.Address;
            _tekushiyPolzovatel.PasswordHash = izmenenniyPolzovatel.PasswordHash;

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            StatusText.Text = "Не удалось сохранить профиль.";
            MessageBox.Show(this, ex.Message, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
