namespace ImageEndpoint.Core;

public class SourceImageNotFoundException : ImageConverterException
{
    public SourceImageNotFoundException() : base("The source image was not found.")
    {
    }
}