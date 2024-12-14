namespace ImageEndpoint.Core;

public class BasicImageConverterHandler : IImageConverterHandler
{
    protected ISourceImagesRepository SourceImagesRepository { get; }
    
    protected  IImageConverter ImageConverter { get; }
    
    protected IConvertedImagesRepository ConvertedImagesRepository { get; }
    
    public BasicImageConverterHandler(
        ISourceImagesRepository sourceImagesRepository,
        IImageConverter imageConverter,
        IConvertedImagesRepository convertedImagesRepository
    )
    {
        SourceImagesRepository = sourceImagesRepository;
        ImageConverter = imageConverter;
        ConvertedImagesRepository = convertedImagesRepository;
    }
    
    public async Task<Stream> HandleAsync(ImageConversionArgs args, CancellationToken cancellationToken)
    {
        ImageUrlEncoding.ValidateChecksum(args);
        
        // Check if converted already exists
        if(await ConvertedImagesRepository.ExistsAsync(args, cancellationToken))
        {
            return await ConvertedImagesRepository.GetFileContentAsync(args, cancellationToken);
        }
        
        // Check if source image exists
        if(!await SourceImagesRepository.ExistsAsync(args, cancellationToken))
        {
            throw new ApplicationException();
        }
        
        // Convert image
        var result = await ImageConverter.ConvertAsync(args, cancellationToken);
        
        if(!result.Success)
        {
            throw result.Exception ?? new ApplicationException();
        }
        
        // Get converted image
        return await ConvertedImagesRepository.GetFileContentAsync(args, cancellationToken);
    }
}