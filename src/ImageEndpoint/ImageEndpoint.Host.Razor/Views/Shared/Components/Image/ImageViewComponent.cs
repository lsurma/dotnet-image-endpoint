using ImageEndpoint.Core;
using Microsoft.AspNetCore.Mvc;

namespace ImageEndpoint.Host.Razor.Views.Shared.Components.Image;

public class ImageViewComponent : ViewComponent
{
    public string Src { get; set; }
    
    public string SrcUrl => $"https://localhost:7117/blob/{Src}".WithImgConversion(200, 200);
    
    public IViewComponentResult Invoke(
        string src   
    )
    {
        Src = src;
        return View(this);
    }
}