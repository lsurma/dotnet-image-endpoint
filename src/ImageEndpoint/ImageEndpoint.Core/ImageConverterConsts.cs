namespace ImageEndpoint.Core;

public class ImageConverterConsts
{
    public class Formats
    {
        public const string Jpeg = "jpeg";
        public const string Png = "png";
        public const string WebP = "webp";
        public const string Avif = "avif";
    }
    
    public class ConversionTypes
    {
        public const string Resize = "resize";
        public const string Crop = "crop";
        public const string Fit = "fit";
        public const string Cover = "cover";
        
        public const string Default = Cover;
    }
    
    public class ContentTypes
    {
        public const string Jpeg = "image/jpeg";
        public const string Png = "image/png";
        public const string WebP = "image/webp";
        public const string Avif = "image/avif";
    }
    
    public class Extensions
    {
        public const string Jpeg = "jpeg";
        public const string Png = "png";
        public const string WebP = "webp";
        public const string Avif = "avif";
    }
}
