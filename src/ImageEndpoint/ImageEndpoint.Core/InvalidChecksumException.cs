namespace ImageEndpoint.Core;

public class InvalidChecksumException : ImageConverterException
{
    public InvalidChecksumException() : base("The checksum of the image data is invalid.")
    {
    }
}