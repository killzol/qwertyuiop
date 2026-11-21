using System.Windows;
using System.Windows.Media;
using искулинария.DbService;
using искулинария.Model;
using искулинария.Views;
using искулинария.Views.Pages;
using искулинария.Views.Windows;

namespace искулинария;

public partial class MainWindow : Window
{
    private BludaFromDB? _rabotaSBazoy;
    private List<Bludo> _vseBlyuda = new List<Bludo>();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_OnLoaded;
        Closed += MainWindow_OnClosed;
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        User? polzovatel = AuthWindow.CurrentUser;
        if (polzovatel == null)
        {
            Close();
            return;
        }

        UserNameText.Text = polzovatel.FullName;
        AddDishButton.Visibility = Visibility.Visible;
        if (polzovatel.RoleId == UserRoles.Manager)
            AddDishButton.Visibility = Visibility.Collapsed;

        UsersTabItem.Visibility = Visibility.Collapsed;
        if (polzovatel.RoleId == UserRoles.Administrator)
            UsersTabItem.Visibility = Visibility.Visible;

        LoadData();
    }

    private void LoadData()
    {
        try
        {
            SetStatus("Подключение к базе…", "#D7F1ED");

            if (_rabotaSBazoy == null)
                _rabotaSBazoy = new BludaFromDB(DbConnect.CreateDataSource());

            CategoryComboBox.ItemsSource = _rabotaSBazoy.LoadCategories();
            CategoryComboBox.SelectedIndex = 0;

            _vseBlyuda = _rabotaSBazoy.LoadBludos();
            ApplyFilters();
            SetStatus("Блюд в базе: " + _vseBlyuda.Count, "#D7F1ED");
        }
        catch (Exception ex)
        {
            SetStatus("Нет подключения к базе", "#FFE0B2");
            EmptyMessage.Text = "Не удалось загрузить блюда. Проверьте appsettings.json и подключение PostgreSQL.";
            EmptyMessage.Visibility = Visibility.Visible;
            MessageBox.Show(this, ex.Message, "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyFilters()
    {
        string poisk = SearchBox.Text.Trim();
        CategoryFilter? kategoriya = CategoryComboBox.SelectedItem as CategoryFilter;
        List<Bludo> spisokBlyud = new List<Bludo>();

        foreach (Bludo bludo in _vseBlyuda)
        {
            if (poisk != "" && !bludo.Name.ToLower().Contains(poisk.ToLower()))
                continue;

            if (kategoriya != null && kategoriya.Id != null && bludo.CategoryId != kategoriya.Id)
                continue;

            spisokBlyud.Add(bludo);
        }

        BludoItems.ItemsSource = spisokBlyud;

        if (_vseBlyuda.Count == 0)
            EmptyMessage.Text = "В таблице блюд пока нет записей.";
        else
            EmptyMessage.Text = "По заданным условиям блюда не найдены.";

        if (spisokBlyud.Count == 0)
            EmptyMessage.Visibility = Visibility.Visible;
        else
            EmptyMessage.Visibility = Visibility.Collapsed;
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
        User? polzovatel = AuthWindow.CurrentUser;
        if (polzovatel == null)
            return;

        ProfileWindow forma = new ProfileWindow(polzovatel);
        forma.Owner = this;

        if (forma.ShowDialog() == true)
            UserNameText.Text = polzovatel.FullName;
    }

    private void ExitButton_OnClick(object sender, RoutedEventArgs e)
    {
        AuthWindow.CurrentUser = null;
        AuthWindow forma = new AuthWindow();
        Application.Current.MainWindow = forma;
        forma.Show();
        Close();
    }

    private void MainTabs_OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (MainTabs.SelectedItem == ProductsTabItem && ProductsFrame.Content == null)
            ProductsFrame.Navigate(new ProductsPage());

        if (MainTabs.SelectedItem == UsersTabItem && AdminFrame.Content == null &&
            AuthWindow.CurrentUser != null &&
            AuthWindow.CurrentUser.RoleId == UserRoles.Administrator)
        {
            AdminFrame.Navigate(new AdminPage());
        }
    }

    private void SetStatus(string message, string colorHex)
    {
        ConnectionStatus.Text = message;
        ConnectionStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
    }

    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        if (_rabotaSBazoy != null)
            _rabotaSBazoy.Dispose();
    }
}
