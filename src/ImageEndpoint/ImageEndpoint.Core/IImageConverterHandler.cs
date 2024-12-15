namespace ImageEndpoint.Core;

public interface IImageConverterHandler
{
    Task<ImageConverterHandlerResult> HandleAsync(ImageConversionInputArgs args, CancellationToken cancellationToken);
}