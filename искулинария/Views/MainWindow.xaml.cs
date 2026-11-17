using System.Windows;
using искулинария.DbService;
using искулинария.Model;
using искулинария.Views;
using искулинария.Views.Pages;

namespace искулинария;

public partial class MainWindow : Window
{
    private BludaFromDB? _repository;
    private List<Bludo> _allBludos = [];

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_OnLoaded;
        Closed += MainWindow_OnClosed;
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        var currentUser = AuthWindow.CurrentUser;
        if (currentUser is null)
        {
            Close();
            return;
        }

        UserNameText.Text = currentUser.FullName;
        AddDishButton.Visibility = currentUser.RoleId == UserRoles.Manager ? Visibility.Collapsed : Visibility.Visible;
        UsersTabItem.Visibility = currentUser.RoleId == UserRoles.Administrator ? Visibility.Visible : Visibility.Collapsed;

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            SetStatus("Подключение к базе…", "#D7F1ED");
            _repository ??= new BludaFromDB(DbConnect.CreateDataSource());

            var categories = await _repository.LoadCategoriesAsync();
            CategoryComboBox.ItemsSource = categories;
            CategoryComboBox.SelectedIndex = 0;

            _allBludos = await _repository.LoadBludosAsync();
            ApplyFilters();
            SetStatus($"Блюд в базе: {_allBludos.Count}", "#D7F1ED");
        }
        catch (Exception ex)
        {
            SetStatus("Нет подключения к базе", "#FFE0B2");
            EmptyMessage.Text = "Не удалось загрузить блюда. Проверьте appsettings.json и подключение PostgreSQL.";
            EmptyMessage.Visibility = Visibility.Visible;
            MessageBox.Show(
                this,
                $"Не удалось загрузить данные из PostgreSQL.\n\n{ex.Message}",
                "Ошибка подключения",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ApplyFilters()
    {
        var query = SearchBox.Text.Trim();
        var selectedCategory = CategoryComboBox.SelectedItem as CategoryFilter;

        var filtered = _allBludos.Where(bludo =>
            (string.IsNullOrWhiteSpace(query) || bludo.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase)) &&
            (selectedCategory?.Id is null || bludo.CategoryId == selectedCategory.Id));

        var result = filtered.ToList();
        BludoItems.ItemsSource = result;
        EmptyMessage.Text = _allBludos.Count == 0
            ? "В таблице bluda пока нет записей."
            : "По заданным условиям блюда не найдены.";
        EmptyMessage.Visibility = result.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SearchBox_OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void CategoryComboBox_OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        ApplyFilters();
    }

    private async void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private void ExitButton_OnClick(object sender, RoutedEventArgs e)
    {
        AuthWindow.CurrentUser = null;
        var authWindow = new AuthWindow();
        Application.Current.MainWindow = authWindow;
        authWindow.Show();
        Close();
    }

    private void MainTabs_OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ReferenceEquals(MainTabs.SelectedItem, UsersTabItem) &&
            AuthWindow.CurrentUser?.RoleId == UserRoles.Administrator &&
            AdminFrame.Content is null)
        {
            AdminFrame.Navigate(new AdminPage());
        }
    }

    private void SetStatus(string message, string colorHex)
    {
        ConnectionStatus.Text = message;
        ConnectionStatus.Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(colorHex)!;
    }

    private async void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        if (_repository is not null)
            await _repository.DisposeAsync();
    }
}
