using System.Windows.Media;

namespace искулинария.Model;

public sealed class Bludo
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int? CategoryId { get; init; }
    public string CategoryName { get; init; } = "Без категории";
    public string Base { get; init; } = string.Empty;
    public int? Yield { get; init; }
    public string Recipe { get; init; } = string.Empty;
    public ImageSource Image { get; init; } = default!;
}

public sealed class CategoryFilter
{
    public int? Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
