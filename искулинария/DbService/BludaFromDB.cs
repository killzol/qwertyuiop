using искулинария.Domain;
using искулинария.Model;
using Npgsql;

namespace искулинария.DbService;

public sealed class BludaFromDB(NpgsqlDataSource dataSource) : IAsyncDisposable
{
    public async Task<List<CategoryFilter>> LoadCategoriesAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT category_id, category_name
            FROM public.category_bluda
            ORDER BY category_name;
            """;

        var categories = new List<CategoryFilter>
        {
            new() { Id = null, Name = "Все категории" }
        };

        await using var command = dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            categories.Add(new CategoryFilter
            {
                Id = reader.GetInt32(0),
                Name = reader.IsDBNull(1) ? "Без названия" : reader.GetString(1)
            });
        }

        return categories;
    }

    public async Task<List<Bludo>> LoadBludosAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT b.bludo_id,
                   b.bludo_name,
                   b.id_categirya,
                   COALESCE(c.category_name, 'Без категории') AS category_name,
                   b.osnova,
                   b.vihod,
                   b.bludo_image,
                   COALESCE(r.rezept, '') AS rezept
            FROM public.bluda AS b
            LEFT JOIN public.category_bluda AS c
                ON c.category_id = b.id_categirya
            LEFT JOIN LATERAL (
                SELECT rezept
                FROM public.rezepty
                WHERE bludo_id = b.bludo_id
                LIMIT 1
            ) AS r ON TRUE
            ORDER BY b.bludo_name;
            """;

        var bludos = new List<Bludo>();
        await using var command = dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            bludos.Add(new Bludo
            {
                Id = reader.GetInt32(0),
                Name = reader.IsDBNull(1) ? "Без названия" : reader.GetString(1),
                CategoryId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                CategoryName = reader.GetString(3),
                Base = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                Yield = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                Recipe = reader.GetString(7),
                Image = ImageWorker.Load(reader.IsDBNull(6) ? null : reader.GetString(6))
            });
        }

        return bludos;
    }

    public ValueTask DisposeAsync() => dataSource.DisposeAsync();
}
