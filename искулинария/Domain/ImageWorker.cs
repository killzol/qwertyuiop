using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace искулинария.Domain;

public static class ImageWorker
{
    public static ImageSource Load(string? fileName)
    {
        var safeName = string.IsNullOrWhiteSpace(fileName)
            ? "picture.jpg"
            : System.IO.Path.GetFileName(fileName);

        try
        {
            return LoadResource(safeName);
        }
        catch (IOException)
        {
            return LoadResource("picture.jpg");
        }
        catch (InvalidOperationException)
        {
            return LoadResource("picture.jpg");
        }
    }

    private static BitmapImage LoadResource(string fileName)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri($"pack://application:,,,/искулинария;component/Images/{Uri.EscapeDataString(fileName)}", UriKind.Absolute);
        image.EndInit();
        image.Freeze();
        return image;
    }
}
