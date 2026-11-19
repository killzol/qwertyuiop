using System.Windows;
using искулинария.DbService;
using искулинария.Model;

namespace искулинария.Views.Windows;

public partial class ProfileWindow : Window
{
    private readonly User _currentUser;
    private bool _isPasswordChanged;

    public ProfileWindow(User currentUser)
    {
        InitializeComponent();
        _currentUser = currentUser;
    }

    private void ProfileWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        FirstNameBox.Text = _currentUser.FirstName;
        LastNameBox.Text = _currentUser.LastName;
        PatronymicBox.Text = _currentUser.Patronymic;
        BirthdayPicker.SelectedDate = _currentUser.DateOfBirthday;
        LoginBox.Text = _currentUser.Login;
        PhoneBox.Text = _currentUser.Phone;
        AddressBox.Text = _currentUser.Address;

        OldPasswordBox.Clear();
        NewPasswordBox.Clear();
        ConfirmPasswordBox.Clear();
    }

    private void ChangePasswordButton_OnClick(object sender, RoutedEventArgs e)
    {
        _isPasswordChanged = true;
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
        var firstName = FirstNameBox.Text.Trim();
        var lastName = LastNameBox.Text.Trim();

        if (firstName.Length == 0 || lastName.Length == 0)
        {
            StatusText.Text = "Заполните имя и фамилию.";
            return;
        }

        var newPasswordHash = _currentUser.PasswordHash;
        if (_isPasswordChanged)
        {
            var oldPassword = OldPasswordBox.Password;
            var newPassword = NewPasswordBox.Password;
            var repeatedPassword = ConfirmPasswordBox.Password;

            if (oldPassword.Length == 0 || newPassword.Length == 0 || repeatedPassword.Length == 0)
            {
                StatusText.Text = "Заполните все поля для смены пароля.";
                return;
            }

            if (!Verification.VerifySHA512Hash(oldPassword, _currentUser.PasswordHash))
            {
                StatusText.Text = "Старый пароль введён неверно.";
                return;
            }

            if (!UserFromDb.IsPasswordValid(newPassword))
            {
                StatusText.Text = "Новый пароль не соответствует требованиям.";
                return;
            }

            if (newPassword != repeatedPassword)
            {
                StatusText.Text = "Новый пароль и подтверждение не совпадают.";
                return;
            }

            newPasswordHash = Verification.GetSHA512Hash(newPassword);
        }

        var updatedUser = new User
        {
            UserId = _currentUser.UserId,
            FirstName = firstName,
            LastName = lastName,
            Patronymic = PatronymicBox.Text.Trim(),
            DateOfBirthday = BirthdayPicker.SelectedDate,
            Login = LoginBox.Text.Trim(),
            Phone = PhoneBox.Text.Trim(),
            Address = AddressBox.Text.Trim(),
            RoleId = _currentUser.RoleId,
            RoleName = _currentUser.RoleName,
            PasswordHash = newPasswordHash
        };

        SaveButton.IsEnabled = false;
        try
        {
            using var dataSource = DbConnect.CreateDataSource();
            var users = new UserFromDb(dataSource);
            users.UserUpdateProfil(updatedUser, _isPasswordChanged);

            _currentUser.FirstName = updatedUser.FirstName;
            _currentUser.LastName = updatedUser.LastName;
            _currentUser.Patronymic = updatedUser.Patronymic;
            _currentUser.DateOfBirthday = updatedUser.DateOfBirthday;
            _currentUser.Login = updatedUser.Login;
            _currentUser.Phone = updatedUser.Phone;
            _currentUser.Address = updatedUser.Address;
            _currentUser.PasswordHash = updatedUser.PasswordHash;

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            StatusText.Text = "Не удалось сохранить профиль.";
            MessageBox.Show(this, ex.Message, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}