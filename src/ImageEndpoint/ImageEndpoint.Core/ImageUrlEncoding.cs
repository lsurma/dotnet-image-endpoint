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
        ConversionType conversionType = ConversionType.Cover,
        ImageFileFormat? targetFormat = null, 
        int? quality = 100
    )
    {
        // Take last part from url as source image id
        var parts = currentUrl.Split('/');
        var sourceImageId = parts[^1];
        
        return UrlWithConversion(currentUrl,sourceImageId, targetWidth, targetHeight, conversionType, targetFormat, quality);
    }
    
    public static string WithImgConversion(
        this string currentUrl, 
        string sourceImageId,
        int targetWidth, 
        int targetHeight, 
        ConversionType conversionType = ConversionType.Cover,
        ImageFileFormat? targetFormat = null, 
        int? quality = 100
    )
    {
        return UrlWithConversion(currentUrl,sourceImageId, targetWidth, targetHeight, conversionType, targetFormat, quality);
    }
    
    public static string WithImgConversion(
        this Uri uri, 
        string sourceImageId,
        int targetWidth, 
        int targetHeight, 
        ConversionType conversionType = ConversionType.Cover,
        ImageFileFormat? targetFormat = null, 
        int? quality = 100
    )
    {
        return UrlWithConversion(uri.ToString(), sourceImageId, targetWidth, targetHeight, conversionType, targetFormat, quality);
    }
    
    public static string UrlWithConversion(
        string currentUrl, 
        string sourceImageId,
        int targetWidth, 
        int targetHeight, 
        ConversionType conversionType = ConversionType.Cover,
        ImageFileFormat? targetFormat = null, 
        int? quality = 100
    )
    {
        var args = new ImageConversionInputArgs(
            sourceImageId,
            targetWidth,
            targetHeight,
            conversionType,
            targetFormat,
            quality
        );
        
        
        var url = new Uri(currentUrl);
        var query = HttpUtility.ParseQueryString(url.Query);
        query[WidthParamName] = args.Width.ToString();
        query[HeightParamName] = args.Height.ToString();
        
        if(args.TargetFormat != null)
        {
            query[FormatParamName] = args.TargetFormat.ToString()?.ToLower();
        }
        
        query[ConversionTypeParamName] = args.Type.ToString().ToLower();
        query[QualityParamName] = args.Quality.ToString();
        query[ChecksumParamName] = ChecksumFromArgs(args);
        
        var builder = new UriBuilder(url);
        builder.Query = query.ToString();
        return builder.ToString();
    }
    
    
    public static ImageConversionInputArgs ImageConversionArgsFromUrl(
        string sourceImageId,
        IEnumerable<KeyValuePair<string, StringValues>> query
    )
    {
        var dictionary = query.ToDictionary(x => x.Key, x => x.Value);
    
        var targetWidth = Convert.ToInt32(dictionary.GetValueOrDefault(WidthParamName).FirstOrDefault());
        var targetHeight = Convert.ToInt32(dictionary.GetValueOrDefault(HeightParamName).FirstOrDefault());

        ImageFileFormat? targetFormat = null;
        if (dictionary.TryGetValue(FormatParamName, out var value))
        {
            targetFormat = FormatFromString(value);
        }
        
        ConversionType? conversionType = null;
        if (dictionary.TryGetValue(ConversionTypeParamName, out value))
        {
            conversionType = TypeFromString(value);
        }
        else
        {
            throw new ArgumentException("Conversion type is required");
        }
        
        var quality = Convert.ToInt32(dictionary.GetValueOrDefault(QualityParamName).FirstOrDefault());
        var checksum = dictionary.GetValueOrDefault(ChecksumParamName).FirstOrDefault();
        
        var input = new ImageConversionInputArgs(
            sourceImageId,
            targetWidth,
            targetHeight,
            conversionType.Value,
            targetFormat,
            quality
        )
        {
            Checksum = checksum
        };
        
        ValidateChecksum(input);
        
        return input;
    }
    
    public static ImageFileFormat FormatFromString(string? format)
    {
        format = format?.ToLower();
        return format switch
        {
            "png" => ImageFileFormat.Png,
            "jpeg" => ImageFileFormat.Jpeg,
            "webp" => ImageFileFormat.Webp,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
    
    public static ConversionType TypeFromString(string? type)
    {
        type = type?.ToLower();
        return type switch
        {
            "resize" => ConversionType.Resize,
            "crop" => ConversionType.Crop,
            "fit" => ConversionType.Fit,
            "cover" => ConversionType.Cover,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
    public static string ChecksumFromArgs(ImageConversionInputArgs args)
    {
        var json = JsonSerializer.Serialize(args);
        var bytes = Encoding.UTF8.GetBytes(json + ChecksumSecret);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public static bool ChecksumIsValid(ImageConversionInputArgs args)
    {
        return ChecksumFromArgs(args) == args.Checksum;
    }
    
    public static bool ValidateChecksum(ImageConversionInputArgs args)
    {
        var isValid = ChecksumIsValid(args);
        
        if(!isValid)
        {
            throw new InvalidChecksumException();
        }
        
        return isValid;
    }
}