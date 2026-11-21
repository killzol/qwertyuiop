using System.Windows;
using System.Windows.Controls;
using искулинария.DbService;
using искулинария.Model;
using искулинария.Views.Windows;

namespace искулинария.Views.Pages;

public partial class AdminPage : Page
{
    public AdminPage()
    {
        InitializeComponent();
        Loaded += AdminPage_OnLoaded;
    }

    private void AdminPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            LoadUsers();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void LoadUsers()
    {
        using var podklyuchenie = DbConnect.CreateDataSource();
        var polzovateli = new UserFromDb(podklyuchenie);

        UsersGrid.ItemsSource = polzovateli.LoadAllUsers();
        RoleComboBox.ItemsSource = polzovateli.LoadRoles();
        StatusText.Text = "Пользователей: " + UsersGrid.Items.Count;
    }

    private void UsersGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var polzovatel = UsersGrid.SelectedItem as User;

        if (polzovatel == null)
        {
            FirstNameBox.Text = "";
            LastNameBox.Text = "";
            PatronymicBox.Text = "";
            LoginBox.Text = "";
            PhoneBox.Text = "";
            AddressBox.Text = "";
            RoleComboBox.SelectedValue = null;
            RoleComboBox.IsEnabled = false;
            ChangeRoleButton.IsEnabled = false;
            ResetPasswordButton.IsEnabled = false;
            DeleteUserButton.IsEnabled = false;
            return;
        }

        FirstNameBox.Text = polzovatel.FirstName;
        LastNameBox.Text = polzovatel.LastName;
        PatronymicBox.Text = polzovatel.Patronymic;
        LoginBox.Text = polzovatel.Login;
        PhoneBox.Text = polzovatel.Phone;
        AddressBox.Text = polzovatel.Address;
        RoleComboBox.SelectedValue = polzovatel.RoleId;
        RoleComboBox.IsEnabled = true;
        ChangeRoleButton.IsEnabled = true;
        ResetPasswordButton.IsEnabled = true;
        DeleteUserButton.IsEnabled = true;
    }

    private void AddUserButton_OnClick(object sender, RoutedEventArgs e)
    {
        var forma = new RegistrationForm(true);
        forma.Owner = Window.GetWindow(this);

        if (forma.ShowDialog() == true)
        {
            try
            {
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

    private void ChangeRoleButton_OnClick(object sender, RoutedEventArgs e)
    {
        var polzovatel = UsersGrid.SelectedItem as User;
        if (polzovatel == null || RoleComboBox.SelectedValue == null)
            return;

        try
        {
            using var podklyuchenie = DbConnect.CreateDataSource();
            var polzovateli = new UserFromDb(podklyuchenie);
            polzovateli.UpdateUserRole(polzovatel.UserId, (int)RoleComboBox.SelectedValue);
            LoadUsers();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void DeleteUserButton_OnClick(object sender, RoutedEventArgs e)
    {
        var polzovatel = UsersGrid.SelectedItem as User;
        if (polzovatel == null)
            return;

        try
        {
            using var podklyuchenie = DbConnect.CreateDataSource();
            var polzovateli = new UserFromDb(podklyuchenie);
            polzovateli.DeleteUser(polzovatel.UserId);
            LoadUsers();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void ResetPasswordButton_OnClick(object sender, RoutedEventArgs e)
    {
        var polzovatel = UsersGrid.SelectedItem as User;
        if (polzovatel == null)
            return;

        var forma = new ResetPasswordWindow(polzovatel);
        forma.Owner = Window.GetWindow(this);

        if (forma.ShowDialog() == true)
            StatusText.Text = "Пароль пользователя " + polzovatel.FullName + " изменён.";
    }
}
