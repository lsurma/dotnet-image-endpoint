using ImageEndpoint.Core;
using Microsoft.AspNetCore.Mvc;

namespace ImageEndpoint.Host.Razor.Views.Shared.Components.Image;

public class ImageViewComponent : ViewComponent
{
    public string Src { get; set; }

    public (int Width, int Height)[] Sizes { get; set; } = [];

    public string[] Sources => Sizes.Select(size => $"https://localhost:7117/blob/{Src}".WithImgConversion(size.Width, size.Height)).ToArray();
    
    public string MainSrc => Sources.First();

    public string SrcSet => String.Join(", ", Sources.Select((src, i) => $"{src} {Sizes[i].Width}w"));
    
    public IViewComponentResult Invoke(
        string src,
        (int Width, int Height)[] sizes
    )
    {
        Src = src;
        Sizes = sizes;
        return View(this);
    }

    
}