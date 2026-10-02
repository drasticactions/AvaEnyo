using System.Collections.Generic;
using AvaEnyo.Gallery.Pages.Media;

namespace AvaEnyo.Gallery;

public static partial class GalleryPages
{
    static partial void AddMedia(List<GalleryPage> pages)
    {
        const string g = "Media";
        pages.Add(new(g, "Image View", "Swipe between photos; pinch or double-tap to zoom", () => new ImageViewPage(), "src/Media/ImageViewPage.axaml", Scrolls: false,
            Notes: "ImageView is a virtual carousel of ZoomableImages. Double-tap within 300 ms zooms between fit and twice fit in 350 ms; a zoomed image pans with a 100 px spring overscroll, and gives horizontal drags back to the carousel at its edge."));
        pages.Add(new(g, "Croppable Image", "A fixed crop frame with a zoom slider", () => new CroppableImagePage(), "src/Media/CroppableImagePage.axaml", Scrolls: false,
            Notes: "CroppableImage keeps the image covering the frame. Crop() returns the framed region as a bitmap."));
    }
}
