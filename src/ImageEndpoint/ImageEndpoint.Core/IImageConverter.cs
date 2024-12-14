namespace ImageEndpoint.Core;

public interface IImageConverter
{
    Task<ImageConversionResult> ConvertAsync(ImageConversionArgs args, CancellationToken cancellationToken);
}