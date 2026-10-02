using System.Collections.Generic;
using AvaEnyo.Gallery.Pages.Forms;

namespace AvaEnyo.Gallery;

public static partial class GalleryPages
{
    static partial void AddForms(List<GalleryPage> pages)
    {
        pages.Add(new("Forms", "Forms & Rows", "The Enyo Sampler page: inputs with accessories in rows and row groups", () => new FormsPage(), "src/Forms/FormsPage.axaml",
            Notes: "Inputs draw no frame until focused. In a RowGroup the first, middle and last rows get matching focus frames. A DividerDrawer opens in 250 ms and closes in 100 ms."));
    }
}
