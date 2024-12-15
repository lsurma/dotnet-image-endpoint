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
            args.TargetFormat.ToString(),
            args.Quality?.ToString() ?? "default",
            args.Type.ToString()
        };

        var key = String.Join("-", parts);
        key = key.Replace(".", "-");
        key = key.ToLower();
        key += FormatToExt(args.TargetFormat);
        
        return key;
    }
    
    
    public ImageFileFormat ExtToFormat(string ext)
    {
        ext = ext.ToLower();
        ext = ext.Replace(".", "");
        
        return ext switch
        {
            "jpg" => ImageFileFormat.Jpeg,
            "png" => ImageFileFormat.Png,
            "webp" => ImageFileFormat.Webp,
            _ => throw new ArgumentOutOfRangeException(nameof(ext), ext, null)
        };
    }
    
    public string FormatToExt(ImageFileFormat format)
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