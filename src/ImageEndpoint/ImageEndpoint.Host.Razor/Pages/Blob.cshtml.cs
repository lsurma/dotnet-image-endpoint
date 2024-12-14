using ImageEndpoint.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ImageEndpoint.Host.Razor.Pages;

public class Blob : PageModel
{
    [FromRoute]
    public string Id { get; set; }
    
    [FromQuery]
    public ImageConversionArgs ConversionArgs { get; set; }
    
    public IImageSourceRepository ImageSourceRepository { get; }
    public ImageManipulator ImageManipulator { get; }
    public IManipulatedImageRepository ManipulatedImageRepository { get; }

    public Blob(
        IImageSourceRepository imageSourceRepository,
        ImageManipulator imageManipulator,
        IManipulatedImageRepository manipulatedImageRepository
    )
    {
        ImageSourceRepository = imageSourceRepository;
        ImageManipulator = imageManipulator;
        ManipulatedImageRepository = manipulatedImageRepository;
    }
    
    public async Task<IActionResult> OnGetAsync()
    {
        var exists = await ImageSourceRepository.FileExistsAsync(Id);
        
        if(!exists)
        {
            return NotFound();
        }
        
        ConversionArgs.SourceImageId = Id;
        var key = ImageManipulator.GetTaskKey(ConversionArgs);
        
        if(await ManipulatedImageRepository.FileExistsAsync(key))
        {
            var stream = await ManipulatedImageRepository.GetFileContentByIdAsync(key);
            return File(stream, GetContentTypeFromFileName(ConversionArgs.Format), Id);
        }
        
        var manipulationResult = await ImageManipulator.ConvertAsync(ConversionArgs, HttpContext.RequestAborted);
        
        if(!manipulationResult)
        {
            return BadRequest();
        }
        
        if(await ManipulatedImageRepository.FileExistsAsync(key))
        {
            var stream = await ManipulatedImageRepository.GetFileContentByIdAsync(key);
            return File(stream, GetContentTypeFromFileName(ConversionArgs.Format), Id);
        }
        
        return BadRequest();
    }
    
    private string GetContentTypeFromFileName(ImageFileFormat id)
    {
        return id switch
        {
            ImageFileFormat.Jpeg => "image/jpeg",
            ImageFileFormat.Png => "image/png",
            ImageFileFormat.Webp => "image/webp",
            _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
        };
    }
}