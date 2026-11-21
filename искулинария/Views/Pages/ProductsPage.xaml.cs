using System.Windows;
using System.Windows.Controls;
using искулинария.DbService;
using искулинария.Model;
using искулинария.Views;
using искулинария.Views.Windows;

namespace искулинария.Views.Pages;

public partial class ProductsPage : Page
{

    public ProductsPage()
    {
        InitializeComponent();
        Loaded += ProductsPage_OnLoaded;
    }

    private bool CanEditProducts()
    {
        User? polzovatel = AuthWindow.CurrentUser;
        if (polzovatel == null)
            return false;

        return polzovatel.RoleId == UserRoles.Administrator ||
               polzovatel.RoleId == UserRoles.Manager;
    }
    private void ProductsPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!CanEditProducts())
            ProductButtons.Visibility = Visibility.Collapsed;

        LoadProducts();
    }

    private void LoadProducts()
    {
        try
        {
            using var podklyuchenie = DbConnect.CreateDataSource();
            ProduktyFromDB produkty = new ProduktyFromDB(podklyuchenie);
            ProductsGrid.ItemsSource = produkty.LoadProducts();

            if (ProductsGrid.Items.Count == 0)
                StatusText.Text = "Продуктов пока нет.";
            else
                StatusText.Text = "Продуктов: " + ProductsGrid.Items.Count;
        }
        catch (Exception ex)
        {
            StatusText.Text = "Не удалось загрузить продукты.";
            MessageBox.Show(Window.GetWindow(this), ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddProductButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!CanEditProducts())
            return;

        ProduktAddWindow forma = new ProduktAddWindow();
        forma.Owner = Window.GetWindow(this);

        if (forma.ShowDialog() == true)
            LoadProducts();
    }

    private void DeleteProductButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!CanEditProducts())
            return;

        Produkt? produkt = ProductsGrid.SelectedItem as Produkt;
        if (produkt == null)
        {
            MessageBox.Show(Window.GetWindow(this), "Сначала выберите продукт.", "Удаление продукта", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        MessageBoxResult otvet = MessageBox.Show(
            Window.GetWindow(this),
            "Удалить продукт " + produkt.Nazvanie + "?",
            "Удаление продукта",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (otvet != MessageBoxResult.Yes)
            return;

        try
        {
            using var podklyuchenie = DbConnect.CreateDataSource();
            ProduktyFromDB produkty = new ProduktyFromDB(podklyuchenie);
            produkty.DeleteProduct(produkt.ProduktId);
            LoadProducts();
        }
        catch (Exception ex)
        {
            MessageBox.Show(Window.GetWindow(this), ex.Message, "Не удалось удалить продукт", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
