using Avalonia.Controls;

namespace AvaEnyo.Controls.Tests;

public static partial class ThemedControls
{
    static partial void AddMedia()
    {
        Add("SnapScroller", new Row("Carousel", () => new SnapScroller { Items = { new Border { Width = 200 }, new Border { Width = 200 } } }, ["PART_Strip", "PART_ItemsPresenter"], false));
        Add("Carousel", new Row("Carousel", () =>
        {
            var c = new AvaEnyo.Controls.Carousel();
            c.SetCenterView("center");
            return c;
        }, ["PART_Strip", "PART_Left", "PART_Center", "PART_Right"], false));
        Add("VirtualCarousel", new Row("Carousel", () => new VirtualCarousel { ItemsSource = new[] { "a", "b", "c" } }, ["PART_Strip", "PART_Left", "PART_Center", "PART_Right"], false));
        Add("ImageView", new Row("Carousel", () => new ImageView { Images = new object[] { "avares://AvaEnyo.Controls/missing.png" } }, ["PART_Strip", "PART_Left", "PART_Center", "PART_Right"], false));
    }
}
