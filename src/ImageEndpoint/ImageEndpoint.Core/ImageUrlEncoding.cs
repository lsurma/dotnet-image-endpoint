using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Primitives;

namespace ImageEndpoint.Core;

public static class ImageUrlEncoding
{
    public static string ChecksumSecret = "SuperSecretChecksumSecret";
    
    public static string WidthParamName = "w";
    public static string HeightParamName = "h";
    public static string FormatParamName = "f";
    public static string ConversionTypeParamName = "t";
    public static string QualityParamName = "q";
    public static string ChecksumParamName = "k";
        
    public static string WithImgConversion(
        this string currentUrl, 
        int targetWidth, 
        int targetHeight, 
        ImageFileFormat targetFormat = ImageFileFormat.Webp, 
        ConversionType conversionType = ConversionType.Cover,
        int? quality = 100
    )
    {
        // Take last part from url as source image id
        var parts = currentUrl.Split('/');
        var sourceImageId = parts[^1];
        
        return UrlWithConversion(currentUrl,sourceImageId, targetWidth, targetHeight, targetFormat, conversionType, quality);
    }
    
    public static string WithImgConversion(
        this string currentUrl, 
        string sourceImageId,
        int targetWidth, 
        int targetHeight, 
        ImageFileFormat targetFormat = ImageFileFormat.Webp, 
        ConversionType conversionType = ConversionType.Cover,
        int? quality = 100
    )
    {
        return UrlWithConversion(currentUrl,sourceImageId, targetWidth, targetHeight, targetFormat, conversionType, quality);
    }
    
    public static string WithImgConversion(
        this Uri uri, 
        string sourceImageId,
        int targetWidth, 
        int targetHeight, 
        ImageFileFormat targetFormat = ImageFileFormat.Webp, 
        ConversionType conversionType = ConversionType.Cover,
        int? quality = 100
    )
    {
        return UrlWithConversion(uri.ToString(), sourceImageId, targetWidth, targetHeight, targetFormat, conversionType, quality);
    }
    
    public static string UrlWithConversion(
        string currentUrl, 
        string sourceImageId,
        int targetWidth, 
        int targetHeight, 
        ImageFileFormat targetFormat = ImageFileFormat.Webp, 
        ConversionType conversionType = ConversionType.Cover,
        int? quality = 100
    )
    {
        var args = new ImageConversionArgs(
            sourceImageId,
            targetWidth,
            targetHeight,
            targetFormat,
            conversionType,
            quality
        );
        
        
        var url = new Uri(currentUrl);
        var query = HttpUtility.ParseQueryString(url.Query);
        query[WidthParamName] = args.Width.ToString();
        query[HeightParamName] = args.Height.ToString();
        query[FormatParamName] = args.Format.ToString().ToLower();
        query[ConversionTypeParamName] = args.Type.ToString().ToLower();
        query[QualityParamName] = args.Quality.ToString();
        query[ChecksumParamName] = ChecksumFromArgs(args);
        
        var builder = new UriBuilder(url);
        builder.Query = query.ToString();
        return builder.ToString();
    }
    
    
    public static ImageConversionArgs ImageConversionArgsFromUrl(
        string sourceImageId,
        IEnumerable<KeyValuePair<string, StringValues>> query
    )
    {
        var dictionary = query.ToDictionary(x => x.Key, x => x.Value);
    
        var targetWidth = Convert.ToInt32(dictionary.GetValueOrDefault(WidthParamName).FirstOrDefault());
        var targetHeight = Convert.ToInt32(dictionary.GetValueOrDefault(HeightParamName).FirstOrDefault());
        var targetFormat = FormatFromString(dictionary.GetValueOrDefault(FormatParamName).FirstOrDefault(), ImageFileFormat.Webp);
        var conversionType = TypeFromString(dictionary.GetValueOrDefault(ConversionTypeParamName).FirstOrDefault(), ConversionType.Cover);
        var quality = Convert.ToInt32(dictionary.GetValueOrDefault(QualityParamName).FirstOrDefault());
        var checksum = dictionary.GetValueOrDefault(ChecksumParamName).FirstOrDefault();
        
        return new ImageConversionArgs(
            sourceImageId,
            targetWidth,
            targetHeight,
            targetFormat,
            conversionType,
            quality
        )
        {
            Checksum = checksum
        };
    }
    
    public static ImageFileFormat FormatFromString(string? format, ImageFileFormat defaultFormat)
    {
        format = format?.ToLower();
        return format switch
        {
            "png" => ImageFileFormat.Png,
            "jpeg" => ImageFileFormat.Jpeg,
            "webp" => ImageFileFormat.Webp,
            _ => defaultFormat
        };
    }
    
    public static ConversionType TypeFromString(string? type, ConversionType defaultType)
    {
        type = type?.ToLower();
        return type switch
        {
            "resize" => ConversionType.Resize,
            "crop" => ConversionType.Crop,
            "fit" => ConversionType.Fit,
            "cover" => ConversionType.Cover,
            _ => defaultType
        };
    }
    
    public static string ChecksumFromArgs(ImageConversionArgs args)
    {
        var json = JsonSerializer.Serialize(args);
        var bytes = Encoding.UTF8.GetBytes(json + ChecksumSecret);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public static bool ChecksumIsValid(ImageConversionArgs args)
    {
        return ChecksumFromArgs(args) == args.Checksum;
    }
    
    public static bool ValidateChecksum(ImageConversionArgs args)
    {
        var isValid = ChecksumIsValid(args);
        
        if(!isValid)
        {
            throw new UnauthorizedAccessException("Checksum is invalid");
        }
        
        return isValid;
    }
}