using Npgsql;
using искулинария.Model;

namespace искулинария.DbService;

public class ProduktyFromDB
{
    private readonly NpgsqlDataSource _dataSource;

    public ProduktyFromDB(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public List<Produkt> LoadProducts()
    {
        const string zapros = "SELECT product_id, product_name, belky, jiry, uglevody FROM public.products ORDER BY product_name;";
        List<Produkt> spisokProduktov = new List<Produkt>();

        using var komanda = _dataSource.CreateCommand(zapros);
        using var chitatel = komanda.ExecuteReader();

        while (chitatel.Read())
        {
            Produkt produkt = new Produkt();
            produkt.ProduktId = chitatel.GetInt32(0);

            if (!chitatel.IsDBNull(1))
                produkt.Nazvanie = chitatel.GetString(1);
            if (!chitatel.IsDBNull(2))
                produkt.Belki = chitatel.GetInt32(2);
            if (!chitatel.IsDBNull(3))
                produkt.Zhiry = chitatel.GetInt32(3);
            if (!chitatel.IsDBNull(4))
                produkt.Uglevody = chitatel.GetInt32(4);

            spisokProduktov.Add(produkt);
        }

        return spisokProduktov;
    }

    public void AddProduct(string nazvanie, int belki, int zhiry, int uglevody)
    {
        const string zapros = "INSERT INTO public.products (product_name, belky, jiry, uglevody) VALUES (@name, @protein, @fat, @carbohydrates);";

        using var komanda = _dataSource.CreateCommand(zapros);
        komanda.Parameters.AddWithValue("name", nazvanie.Trim());
        komanda.Parameters.AddWithValue("protein", belki);
        komanda.Parameters.AddWithValue("fat", zhiry);
        komanda.Parameters.AddWithValue("carbohydrates", uglevody);
        komanda.ExecuteNonQuery();
    }

    public void DeleteProduct(int produktId)
    {
        const string proverka = "SELECT EXISTS (SELECT 1 FROM public.sostav_blud WHERE product_id = @id);";
        using var komandaProverki = _dataSource.CreateCommand(proverka);
        komandaProverki.Parameters.AddWithValue("id", produktId);

        if ((bool)komandaProverki.ExecuteScalar()!)
            throw new InvalidOperationException("Этот продукт используется в составе блюда. Удалить его нельзя.");

        const string udalenie = "DELETE FROM public.products WHERE product_id = @id;";
        using var komandaUdaleniya = _dataSource.CreateCommand(udalenie);
        komandaUdaleniya.Parameters.AddWithValue("id", produktId);

        if (komandaUdaleniya.ExecuteNonQuery() == 0)
            throw new InvalidOperationException("Продукт не найден.");
    }
}
