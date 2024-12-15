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
    
    public async Task<ImageConverterHandlerResult> HandleAsync(ImageConversionArgs args, CancellationToken cancellationToken)
    {
        if(!ImageUrlEncoding.ChecksumIsValid(args))
        {
            return new ImageConverterHandlerResult(new InvalidChecksumException());
        }

        // If target format is not specified, then we use the source format
        if (args.TargetFormat is null)
        {
            var sourceFileInfo = await SourceImagesRepository.GetFileInfoAsync(args, cancellationToken);
            args.SetTargetFormat(sourceFileInfo.Format);
        }
        
        if(args.TargetFormat is null)
        {
            return new ImageConverterHandlerResult(new ImageConverterException("Target format is not specified"));
        }
        
        // Check if converted already exists
        if(await ConvertedImagesRepository.ExistsAsync(args, cancellationToken))
        {
            var existingConvertedData = await ConvertedImagesRepository.GetFileContentAsync(args, cancellationToken);
            return new ImageConverterHandlerResult(existingConvertedData, args.TargetFormat.Value);
        }
        
        // Check if source image exists for processing
        if(!await SourceImagesRepository.ExistsAsync(args, cancellationToken))
        {
            return new ImageConverterHandlerResult(new SourceImageNotFoundException());
        }
        
        // Convert image
        var result = await ImageConverter.ConvertAsync(args, cancellationToken);
        
        if(!result.Success)
        {
            return new ImageConverterHandlerResult(result.Exception ?? new ImageConverterException("Unknown error"));
        }
        
        // Get converted image
        var data = await ConvertedImagesRepository.GetFileContentAsync(args, cancellationToken);

        return new ImageConverterHandlerResult(data, args.TargetFormat.Value);
    }
}