using System.Windows;
using искулинария.DbService;
using искулинария.Model;
using искулинария.Views;
using искулинария.Views.Windows;
using искулинария.Views.Pages;

namespace искулинария;

public partial class MainWindow : Window
{
    private BludaFromDB? _rabotaSBazoy;
    private List<Bludo> _vseBlyuda = [];

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_OnLoaded;
        Closed += MainWindow_OnClosed;
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        var tekushiyPolzovatel = AuthWindow.CurrentUser;
        if (tekushiyPolzovatel is null)
        {
            Close();
            return;
        }

        UserNameText.Text = tekushiyPolzovatel.FullName;
        AddDishButton.Visibility = tekushiyPolzovatel.RoleId == UserRoles.Manager ? Visibility.Collapsed : Visibility.Visible;
        UsersTabItem.Visibility = tekushiyPolzovatel.RoleId == UserRoles.Administrator ? Visibility.Visible : Visibility.Collapsed;

        LoadData();
    }

    private void LoadData()
    {
        try
        {
            SetStatus("Подключение к базе…", "#D7F1ED");
            _rabotaSBazoy ??= new BludaFromDB(DbConnect.CreateDataSource());

            var kategorii = _rabotaSBazoy.LoadCategories();
            CategoryComboBox.ItemsSource = kategorii;
            CategoryComboBox.SelectedIndex = 0;

            _vseBlyuda = _rabotaSBazoy.LoadBludos();
            ApplyFilters();
            SetStatus($"Блюд в базе: {_vseBlyuda.Count}", "#D7F1ED");
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
        var poisk = SearchBox.Text.Trim();
        var vybrannayaKategoriya = CategoryComboBox.SelectedItem as CategoryFilter;

        var spisokPosleFiltra = _vseBlyuda.Where(bludo =>
            (string.IsNullOrWhiteSpace(poisk) || bludo.Name.Contains(poisk, StringComparison.CurrentCultureIgnoreCase)) &&
            (vybrannayaKategoriya?.Id is null || bludo.CategoryId == vybrannayaKategoriya.Id));

        var rezultat = spisokPosleFiltra.ToList();
        BludoItems.ItemsSource = rezultat;
        EmptyMessage.Text = _vseBlyuda.Count == 0
            ? "В таблице блюд пока нет записей."
            : "По заданным условиям блюда не найдены.";
        EmptyMessage.Visibility = rezultat.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SearchBox_OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void CategoryComboBox_OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        LoadData();
    }

    private void ProfileButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (AuthWindow.CurrentUser is not { } tekushiyPolzovatel)
            return;

        var profileWindow = new ProfileWindow(tekushiyPolzovatel) { Owner = this };
        if (profileWindow.ShowDialog() == true)
            UserNameText.Text = tekushiyPolzovatel.FullName;
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

    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        if (_rabotaSBazoy is not null)
            _rabotaSBazoy.Dispose();
    }
}
