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
            var stream = await ImageConverterHandler.HandleAsync(args, cancellationToken);
            return File(stream, ImageConversionArgs.ImageFileFormatToContentType(args.Format));
        }
        catch(Exception)
        {
            return BadRequest();
        }
    }
}