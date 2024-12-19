using System.Text.Json.Serialization;
using Microsoft.Extensions.Primitives;

namespace ImageEndpoint.Core;

public class ImageConversionInputArgs
{
    public ImageConversionInputArgs(
        string sourceImageId, 
        int targetWidth, 
        int targetHeight, 
        ConversionType conversionType, 
        ImageFileFormat? targetFormat = null, 
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
    public ImageFileFormat? TargetFormat { get; private set; }
    
    public ConversionType Type { get; }

    public int? Quality { get; }
    
    [JsonIgnore]
    public string? Checksum { get; set; }
    
    public void SetTargetFormat(ImageFileFormat format)
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
        inputArgs.TargetFormat ?? ImageFileFormat.WebP,
        inputArgs.Quality
    )
    {
    }
    
    public ImageConversionArgs(
        string sourceImageId, 
        int targetWidth, 
        int targetHeight, 
        ConversionType conversionType, 
        ImageFileFormat targetFormat, 
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
    
    public ImageFileFormat TargetFormat { get; private set; }
    
    public ConversionType Type { get; }

    public int? Quality { get; }
    
    public static string ImageFileFormatToContentType(ImageFileFormat format)
    {
        return format switch
        {
            ImageFileFormat.Jpeg => "image/jpeg",
            ImageFileFormat.Png => "image/png",
            ImageFileFormat.WebP => "image/webp",
            ImageFileFormat.Avif => "image/avif",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
    
    public ImageConversionArgs SetTargetFormat(ImageFileFormat format)
    {
        TargetFormat = format;
        return this;
    }
}

public enum ImageFileFormat
{
    Png,
    Jpeg,
    WebP,
    Avif
}

public enum ConversionType
{
    Resize,
    Crop,
    Fit,
    Cover
}