namespace ImageEndpoint.Core;

public interface IImageConverterHandler
{
    Task<Stream> HandleAsync(ImageConversionArgs args, CancellationToken cancellationToken);
}