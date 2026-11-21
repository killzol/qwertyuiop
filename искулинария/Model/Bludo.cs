using System.Windows.Media;

namespace искулинария.Model;

public sealed class Bludo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = "Без категории";
    public string Base { get; set; } = string.Empty;
    public int? Yield { get; set; }
    public ImageSource Image { get; set; } = null!;
}

public sealed class CategoryFilter
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
