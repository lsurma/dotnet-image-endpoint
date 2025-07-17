namespace ImageEndpoint.Core;

public class ImageConverterHandlerResult
{
    public bool Success { get; }
    
    public Exception? Exception { get;}
    
    public SuccessResult? ImageResult { get;}

    public ImageConverterHandlerResult(Exception exception)
    {
        Success = false;
        Exception = exception;
    }

    public ImageConverterHandlerResult(Stream stream, string format)
    {
        Success = true;
        ImageResult = new SuccessResult(stream, format);
    }
    
    public class SuccessResult
    {
        public SuccessResult(Stream stream, string format)
        {
            Stream = stream;
            Format = format;
        }
        
        public Stream Stream { get; }
        
        public string Format { get; }
    }
}
