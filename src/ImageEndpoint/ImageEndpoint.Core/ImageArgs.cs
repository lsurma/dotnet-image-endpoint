namespace ImageEndpoint.Core;

public class ImageConversionArgs
{
    public string SourceImageId { get; set; }
    
    public int Width { get; set; }
    
    public int Height { get; set; }
    
    public ImageFileFormat Format { get; set; }
    
    public ConversionType Type { get; set; }
}

public enum ImageFileFormat
{
    Png,
    Jpeg,
    Webp
}

public enum ConversionType
{
    Resize,
    Crop,
    Fit,
    Cover
}