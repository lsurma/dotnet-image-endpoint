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
    
    
    public string ExtToFormat(string ext)
    {
        ext = ext.ToLower();
        ext = ext.Replace(".", "");
        
        return ext switch
        {
            ImageConverterConsts.Extensions.Jpeg => ImageConverterConsts.Formats.Jpeg,
            ImageConverterConsts.Extensions.Png => ImageConverterConsts.Formats.Png,
            ImageConverterConsts.Extensions.WebP => ImageConverterConsts.Formats.WebP,
            ImageConverterConsts.Extensions.Avif => ImageConverterConsts.Formats.Avif,
            _ => throw new ArgumentOutOfRangeException(nameof(ext), ext, null)
        };
        
    }
    
    public string FormatToExt(string format)
    {
        format = format.ToLower();
        
        format = format switch
        {
            ImageConverterConsts.Formats.Jpeg => ImageConverterConsts.Formats.Jpeg,
            ImageConverterConsts.Formats.Png => ImageConverterConsts.Formats.Png,
            ImageConverterConsts.Formats.WebP => ImageConverterConsts.Formats.WebP,
            ImageConverterConsts.Formats.Avif => ImageConverterConsts.Formats.Avif,
        };

        return "." + format;
    }

}