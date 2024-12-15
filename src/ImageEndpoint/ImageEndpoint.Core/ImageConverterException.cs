using System.Collections;

namespace ImageEndpoint.Core;

public class ImageConverterException : ApplicationException
{
    public ImageConverterException(string message) : base(message)
    {
        
    }

    public ImageConverterException(string message, Exception innerException) : base(message, innerException)
    {
        
    }
    
}