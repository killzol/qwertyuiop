using System.Windows;
using искулинария.DbService;

namespace искулинария.Views.Windows;

public partial class ProduktAddWindow : Window
{
    public ProduktAddWindow()
    {
        InitializeComponent();
        Loaded += ProduktAddWindow_OnLoaded;
    }

    private void ProduktAddWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        NameBox.Focus();
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
        string nazvanie = NameBox.Text.Trim();

        if (nazvanie == "")
        {
            StatusText.Text = "Введите название продукта.";
            return;
        }

        int belki;
        int zhiry;
        int uglevody;

        if (!int.TryParse(ProteinBox.Text, out belki) || belki < 0 ||
            !int.TryParse(FatBox.Text, out zhiry) || zhiry < 0 ||
            !int.TryParse(CarbohydratesBox.Text, out uglevody) || uglevody < 0)
        {
            StatusText.Text = "Укажите для белков, жиров и углеводов целые числа от 0.";
            return;
        }

        try
        {
            using var podklyuchenie = DbConnect.CreateDataSource();
            ProduktyFromDB produkty = new ProduktyFromDB(podklyuchenie);
            produkty.AddProduct(nazvanie, belki, zhiry, uglevody);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            StatusText.Text = "Не удалось добавить продукт.";
            MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
