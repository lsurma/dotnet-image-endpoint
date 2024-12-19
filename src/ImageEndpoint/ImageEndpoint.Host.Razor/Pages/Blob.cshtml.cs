using ImageEndpoint.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ImageEndpoint.Host.Razor.Pages;

public class Blob : PageModel
{
    public IImageConverterHandler ImageConverterHandler { get; }
    
    [FromRoute]
    public string Id { get; set; }

    public Blob(
        IImageConverterHandler imageConverterHandler
    )
    {
        ImageConverterHandler = imageConverterHandler;
    }
    
    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var args = ImageUrlEncoding.ImageConversionArgsFromUrl(Id, HttpContext.Request.Query);
            var cancellationToken = HttpContext.RequestAborted;
        
            // Check if user supports webp
            if(args.TargetFormat is null )
            {
                if(HttpContext.Request.Headers.Accept.Any(x => x != null && x.Contains("image/avif")))
                {
                    args.SetTargetFormat(ImageFileFormat.Avif);
                }
                else if (HttpContext.Request.Headers.Accept.Any(x => x != null && x.Contains("image/webp")))
                {
                    args.SetTargetFormat(ImageFileFormat.WebP);
                }
            }
            
            var result = await ImageConverterHandler.HandleAsync(args, cancellationToken);
            
            if(result is { Success: true, ImageResult: not null })
            {
                // Add cache headers 7 days
                Response.Headers.Append("Cache-Control", "public, max-age=604800");
                return File(result.ImageResult.Stream, ImageConversionArgs.ImageFileFormatToContentType(result.ImageResult.Format));
            }
            else
            {
                return BadRequest(result.Exception?.Message ?? "Unknown error");
            }
        }
        catch(Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}