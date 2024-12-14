namespace ImageEndpoint.Core;

public class ImageConversionException : ApplicationException
{
    public ImageConversionException() : base("An error occurred during image conversion.")
    {
        
    }

    public ImageConversionException(Exception innerException) : base("An error occurred during image conversion.", innerException)
    {
        
    }
}