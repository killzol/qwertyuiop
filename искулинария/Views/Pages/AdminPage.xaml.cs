using System.Windows;
using System.Windows.Controls;
using искулинария.DbService;

namespace искулинария.Views.Pages;

public partial class AdminPage : Page
{
    private bool _loaded;

    public AdminPage()
    {
        InitializeComponent();
        Loaded += AdminPage_OnLoaded;
    }

    private async void AdminPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_loaded)
            return;

        _loaded = true;
        try
        {
            await using var dataSource = DbConnect.CreateDataSource();
            var repository = new UserFromDb(dataSource);
            UsersGrid.ItemsSource = await repository.LoadAllUsersAsync();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Не удалось загрузить пользователей: {ex.Message}";
        }
    }
}
