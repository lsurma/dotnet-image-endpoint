namespace ImageEndpoint.Core;

public class ImageConversionResult
{
    public bool Success { get; }
    
    public Exception? Exception { get; }

    public ImageConversionResult(bool success)
    {
        Success = success;
    }
    
    public ImageConversionResult(Exception exception)
    {
        Success = false;
        Exception = exception;
    }
    
    public static ImageConversionResult SuccessResult = new ImageConversionResult(true);
}