using System.Text.Json.Serialization;
using Microsoft.Extensions.Primitives;

namespace ImageEndpoint.Core;

public class ImageConversionArgs
{
    public ImageConversionArgs(
        string sourceImageId, 
        int targetWidth, 
        int targetHeight, 
        ImageFileFormat targetFormat, 
        ConversionType conversionType, 
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
        Format = targetFormat;
        Type = conversionType;
        Quality = quality;
    }
    
    public string SourceImageId { get; }
    
    public int Width { get; }
    
    public int Height { get; }
    
    public ImageFileFormat Format { get; }
    
    public ConversionType Type { get; }

    public int? Quality { get; }
    
    [JsonIgnore]
    public string? Checksum { get; set; }
    
    public static string ImageFileFormatToContentType(ImageFileFormat format)
    {
        return format switch
        {
            ImageFileFormat.Jpeg => "image/jpeg",
            ImageFileFormat.Png => "image/png",
            ImageFileFormat.Webp => "image/webp",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
}

public enum ImageFileFormat
{
    Png,
    Jpeg,
    Webp,
    Auto
}

public enum ConversionType
{
    Resize,
    Crop,
    Fit,
    Cover
}