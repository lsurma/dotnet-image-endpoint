using System.Collections.Concurrent;
using System.Text;
using ImageMagick;

namespace ImageEndpoint.Core;

public class ImageMagickImageConverter : IImageConverter
{
    public static int MaxConcurrentTasks = 50;
    
    protected  ISourceImagesRepository SourceImagesRepository { get; }
    
    protected IConvertedImagesRepository ConvertedImagesRepository { get; }

    protected static readonly ConcurrentDictionary<string, Task<ImageConversionResult>> ConversionTasks = 
        new ConcurrentDictionary<string, Task<ImageConversionResult>>();

    protected static SemaphoreSlim Semaphore { get; set; } = new SemaphoreSlim(0);

    public ImageMagickImageConverter(
        ISourceImagesRepository imageRepository,
        IConvertedImagesRepository manipulatedImageRepository
    )
    {
        SourceImagesRepository = imageRepository;
        ConvertedImagesRepository = manipulatedImageRepository;
        Semaphore = new SemaphoreSlim(MaxConcurrentTasks);
    }

    public async Task<ImageConversionResult> ConvertAsync(ImageConversionArgs args, CancellationToken cancellationToken)
    {
        var key = GetTaskKey(args);
        
        var task = ConversionTasks.GetOrAdd(key, async _ =>
        {
            await Semaphore.WaitAsync(cancellationToken);
            try
            {
                await using var sourceStream = await SourceImagesRepository.GetFileContentAsync(args, cancellationToken);
                await using var destinationStream = new MemoryStream();
                
                ConvertImage(sourceStream, destinationStream, args);
                
                destinationStream.Position = 0;
                await ConvertedImagesRepository.SaveContentAsync(destinationStream, args, cancellationToken);
                return ImageConversionResult.SuccessResult;
            }
            catch (Exception ex)
            {
                throw new ImageConversionException(ex);
            }
            finally
            {
                Semaphore.Release();
            }
        });
        
        var result = await task;
        
        // Remove task from dictionary
        ConversionTasks.TryRemove(key, out _);
        
        return result;
    }
    
    protected string GetTaskKey(ImageConversionArgs args)
    {
        return $"{args.SourceImageId}_{args.Width}_{args.Height}_{args.TargetFormat}_{args.Type}";
    }
    
    protected void ConvertImage(Stream input, Stream output, ImageConversionArgs args)
    {
        using var image = new MagickImage(input);
        var type = args.Type;
        var width = args.Width;
        var height = args.Height;
        var format = args.TargetFormat;
        
        // Resize or crop based on type
        if (type == ConversionType.Crop)
        {
            image.Crop((uint)width, (uint)height, Gravity.Center);
        }
        else if (type == ConversionType.Resize)
        {
            image.Resize((uint)width, (uint)height);
        }
        else if (type == ConversionType.Fit)
        {
            // Resize to fit within dimensions, maintaining aspect ratio
            image.Resize(new MagickGeometry((uint)width, (uint)height)
            {
                IgnoreAspectRatio = false,
                Greater = false,
                Less = true  // Ensures image fits within box
            });
        }
        else if (type == ConversionType.Cover)
        {
            // Original image dimensions
            var originalWidth = image.Width;
            var originalHeight = image.Height;

            // Calculate aspect ratios
            double targetRatio = (double)width / height;
            double imageRatio = (double)originalWidth / originalHeight;

            // Determine scaling dimensions
            int resizeWidth, resizeHeight;
            if (imageRatio > targetRatio)
            {
                // Image is wider than target, scale by height
                resizeHeight = height;
                resizeWidth = (int)(height * imageRatio);
            }
            else
            {
                // Image is taller than target, scale by width
                resizeWidth = width;
                resizeHeight = (int)(width / imageRatio);
            }

            // Resize the image to fill the target area (larger dimensions)
            image.Resize((uint)resizeWidth, (uint)resizeHeight);

            // Calculate crop offsets (center-crop)
            int cropX = (resizeWidth - width) / 2;
            int cropY = (resizeHeight - height) / 2;

            // Crop the image to the exact target dimensions
            image.Crop(new MagickGeometry(cropX, cropY, (uint)width, (uint)height));
        }
        else
        {
            throw new InvalidOperationException("Unsupported conversion type");
        }

        image.Quality = (uint)(args.Quality ?? 100);
        image.Strip();

        switch (format)
        {
            case ImageFileFormat.Jpeg:
                image.Format = MagickFormat.Jpeg;
                break;
            case ImageFileFormat.Png:
                image.Format = MagickFormat.Png;
                break;
            case ImageFileFormat.Webp:
                image.Format = MagickFormat.WebP;
                break;
            default:
                throw new InvalidOperationException("Unsupported format");
        }

        image.Write(output);
    }
    
}