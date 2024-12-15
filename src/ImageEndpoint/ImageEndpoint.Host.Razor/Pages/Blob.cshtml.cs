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
        var args = ImageUrlEncoding.ImageConversionArgsFromUrl(Id, HttpContext.Request.Query);
        var cancellationToken = HttpContext.RequestAborted;

        try
        {
            // Check if user supports webp
            if(args.TargetFormat is null && HttpContext.Request.Headers.Accept.Any(x => x != null && x.Contains("image/webp")))
            {
                args.SetTargetFormat(ImageFileFormat.Webp);
            }
            
            var result = await ImageConverterHandler.HandleAsync(args, cancellationToken);
            
            if(result is { Success: true, ImageResult: not null })
            {
                return File(result.ImageResult.Stream, ImageConversionArgs.ImageFileFormatToContentType(result.ImageResult.Format));
            }
            else
            {
                return BadRequest();
            }
        }
        catch(Exception)
        {
            return BadRequest();
        }
    }
}