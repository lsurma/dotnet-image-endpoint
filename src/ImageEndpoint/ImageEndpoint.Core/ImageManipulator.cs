using System.Collections.Concurrent;
using System.Text;
using ImageMagick;
using SkiaSharp;

namespace ImageEndpoint.Core;

public class ImageManipulator
{
    public IImageSourceRepository ImageRepository { get; }
    public IManipulatedImageRepository ManipulatedImageRepository { get; }

    private static readonly ConcurrentDictionary<string, Task<bool>> Tasks = new ConcurrentDictionary<string, Task<bool>>();
    
    private static readonly SemaphoreSlim Semaphore = new(20); // Limit to 20 concurrent tasks

    public ImageManipulator(
        IImageSourceRepository imageRepository,
        IManipulatedImageRepository manipulatedImageRepository
    )
    {
        ImageRepository = imageRepository;
        ManipulatedImageRepository = manipulatedImageRepository;
    }

    public async Task<bool> ConvertAsync(ImageConversionArgs args, CancellationToken cancellationToken)
    {
        var key = GetTaskKey(args);
        
        var task = Tasks.GetOrAdd(key, async _ =>
        {
            await Semaphore.WaitAsync(cancellationToken);
            try
            {
                var sourceStream = await ImageRepository.GetFileContentByIdAsync(args.SourceImageId);
                await using var destinationStream = new MemoryStream();
                var success = ManipulateImageMagick(sourceStream, destinationStream, args.Width, args.Height, args.Type, args.Format);
                if (!success)
                {
                    throw new InvalidOperationException("Failed to manipulate image");
                }
                destinationStream.Position = 0;
                await ManipulatedImageRepository.SaveFileContentAsync(destinationStream, key);
                return true;
            }
            finally
            {
                Semaphore.Release();
            }
        });
        
        var result = await task;
        
        // Remove task from dictionary
        Tasks.TryRemove(key, out _);
        
        return result;
    }
    
    public string GetTaskKey(ImageConversionArgs args)
    {
        var key = $"{args.SourceImageId}_{args.Width}_{args.Height}_{args.Format}_{args.Type}";
        key = key.Replace(".", "_");
        key += "." + FormatToExtension(args.Format);
        return key;
    }
    
    private string FormatToExtension(ImageFileFormat id)
    {
        return id switch
        {
            ImageFileFormat.Jpeg => "jpg",
            ImageFileFormat.Png => "png",
            ImageFileFormat.Webp => "webp",
            _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
        };
    }

    private bool ManipulateImageMagick(Stream input, Stream output, int width, int height, ConversionType type, ImageFileFormat format)
    {
        try
        {
            using var image = new MagickImage(input);
            
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

            // Optimize and save with appropriate format
            image.Quality = 100;
            image.Strip(); // Remove metadata

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
            return true;

        }
        catch
        {
            return false;
        }
    }
    
    
    
    private bool ManipulateImage(Stream input, Stream output, int width, int height, ConversionType type, ImageFileFormat destinationFormat)
    {
        try
        {
            using var bitmap = SKBitmap.Decode(input);

            var info = new SKImageInfo(width, height);
            using var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;

            if (type == ConversionType.Crop)
            {
                float sourceAspect = (float)bitmap.Width / bitmap.Height;
                float destAspect = (float)width / height;

                SKRect srcRect;
                if (sourceAspect > destAspect)
                {
                    float newWidth = bitmap.Height * destAspect;
                    float offsetX = (bitmap.Width - newWidth) / 2;
                    srcRect = new SKRect(offsetX, 0, offsetX + newWidth, bitmap.Height);
                }
                else
                {
                    float newHeight = bitmap.Width / destAspect;
                    float offsetY = (bitmap.Height - newHeight) / 2;
                    srcRect = new SKRect(0, offsetY, bitmap.Width, offsetY + newHeight);
                }

                var destRect = new SKRect(0, 0, width, height);
                
                canvas.DrawBitmap(bitmap, srcRect, destRect, new SKPaint
                {
                    IsAntialias = true
                });
            }
            else if (type == ConversionType.Resize)
            {
                canvas.DrawBitmap(bitmap, new SKRect(0, 0, width, height), new SKPaint
                {
                    FilterQuality = SKFilterQuality.High,
                    IsAntialias = true
                });
            }

            canvas.Flush();
            using var image = surface.Snapshot();
            using var data = destinationFormat switch
            {
                ImageFileFormat.Webp => image.Encode(SKEncodedImageFormat.Webp, 100),
                ImageFileFormat.Jpeg => image.Encode(SKEncodedImageFormat.Jpeg, 100),
                ImageFileFormat.Png => image.Encode(SKEncodedImageFormat.Png, 100),
                _ => throw new InvalidOperationException("Unsupported format")
            };
            data.SaveTo(output);

            return true;
        }
        catch
        {
            return false;
        }
    }
    
}