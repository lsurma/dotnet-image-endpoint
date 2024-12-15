namespace ImageEndpoint.Core;

public interface IImageConverterHandler
{
    Task<ImageConverterHandlerResult> HandleAsync(ImageConversionArgs args, CancellationToken cancellationToken);
}