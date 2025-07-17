using System.Text.Json.Serialization;
using Microsoft.Extensions.Primitives;

namespace ImageEndpoint.Core;

public class ImageConversionInputArgs
{
    public ImageConversionInputArgs(
        string sourceImageId, 
        int targetWidth, 
        int targetHeight, 
        string conversionType, 
        string? targetFormat = null, 
        int? quality = 100
    )
    {
        if(targetHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetHeight), targetHeight, "Height must be greater than 0");
        }
        
        if(targetWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetWidth), targetWidth, "Width must be greater than 0");
        }
        
        if(quality is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(quality), quality, "Quality must be between 0 and 100");
        }
        
        if(String.IsNullOrWhiteSpace(sourceImageId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(sourceImageId));
        }
        
        SourceImageId = sourceImageId;
        Width = targetWidth;
        Height = targetHeight;
        TargetFormat = targetFormat;
        Type = conversionType;
        Quality = quality;
    }
    
    public string SourceImageId { get; }
    
    public int Width { get; }
    
    public int Height { get; }
    
    /// <summary>
    /// When null, the format will be the same as the source image
    /// </summary>
    public string? TargetFormat { get; private set; }
    
    public string Type { get; }

    public int? Quality { get; }
    
    [JsonIgnore]
    public string? Checksum { get; set; }
    
    public void SetTargetFormat(string format)
    {
        TargetFormat = format;
    }
}

public class ImageConversionArgs
{
    public ImageConversionArgs(ImageConversionInputArgs inputArgs) : this(
        inputArgs.SourceImageId,
        inputArgs.Width,
        inputArgs.Height,
        inputArgs.Type,
        inputArgs.TargetFormat ?? ImageConverterConsts.Formats.Avif,
        inputArgs.Quality
    )
    {
    }
    
    public ImageConversionArgs(
        string sourceImageId, 
        int targetWidth, 
        int targetHeight, 
        string conversionType, 
        string targetFormat, 
        int? quality = 100
    )
    {
        if(targetHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetHeight), targetHeight, "Height must be greater than 0");
        }
        
        if(targetWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetWidth), targetWidth, "Width must be greater than 0");
        }
        
        if(quality is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(quality), quality, "Quality must be between 0 and 100");
        }
        
        if(String.IsNullOrWhiteSpace(sourceImageId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(sourceImageId));
        }
        
        SourceImageId = sourceImageId;
        Width = targetWidth;
        Height = targetHeight;
        TargetFormat = targetFormat;
        Type = conversionType;
        Quality = quality;
    }
    
    public string SourceImageId { get; }
    
    public int Width { get; }
    
    public int Height { get; }
    
    public string TargetFormat { get; private set; }
    
    public string Type { get; }

    public int? Quality { get; }
    
    public static string ImageFileFormatToContentType(string format)
    {
        return format switch
        {
            ImageConverterConsts.Formats.Png => ImageConverterConsts.ContentTypes.Png,
            ImageConverterConsts.Formats.Jpeg => ImageConverterConsts.ContentTypes.Jpeg,
            ImageConverterConsts.Formats.WebP => ImageConverterConsts.ContentTypes.WebP,
            ImageConverterConsts.Formats.Avif => ImageConverterConsts.ContentTypes.Avif,
            _ => throw new ArgumentException("Invalid format", nameof(format))
        };
    }
    
    public ImageConversionArgs SetTargetFormat(string format)
    {
        TargetFormat = format;
        return this;
    }
}
