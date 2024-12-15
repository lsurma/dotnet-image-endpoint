using ImageEndpoint.Core;
using FileInfo=ImageEndpoint.Core.FileInfo;

namespace ImageEndpoint.Host.Razor;

public class ImageRepositoryBase
{
    public string GetFilePath(ImageConversionArgs args)
    {
        var parts = new List<string>
        {
            args.SourceImageId,
            args.Width.ToString(),
            args.Height.ToString(),
            args.TargetFormat?.ToString()?? "default",
            args.Quality?.ToString() ?? "default",
            args.Type.ToString()
        };

        var key = String.Join("-", parts);
        key = key.Replace(".", "-");
        key += ExtensionForFormat(args.TargetFormat ?? ImageFileFormat.Jpeg);
        
        return key;
    }
    
    public string ExtensionForFormat(ImageFileFormat format)
    {
        return format switch
        {
            ImageFileFormat.Jpeg => ".jpg",
            ImageFileFormat.Png => ".png",
            ImageFileFormat.Webp => ".webp",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

}