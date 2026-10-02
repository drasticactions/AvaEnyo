using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// Shows an icon given as an <see cref="IImage"/>, a <see cref="Geometry"/> (filled with
/// <see cref="Foreground"/>), a resource key of a geometry, or a control. Enyo apps supplied their own toolbar icons;
/// this presenter takes any of these forms.
/// </summary>
public class IconPresenter : Decorator
{
    /// <summary>Defines the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<IconPresenter, object?>(nameof(Icon));

    /// <summary>Defines the <see cref="Foreground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<IconPresenter>();

    static IconPresenter()
    {
        IconProperty.Changed.AddClassHandler<IconPresenter>((p, _) => p.Rebuild());
        AffectsMeasure<IconPresenter>(IconProperty);
    }

    /// <summary>The icon.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>The fill of a geometry icon. Inherits the text color.</summary>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    private void Rebuild()
    {
        Child = Icon switch
        {
            null => null,
            Control c => c,
            IImage image => new Image { Source = image, Stretch = Stretch.Uniform },
            Geometry g => MakePath(g),
            string key when this.TryFindResource(key, out var res) && res is Geometry g => MakePath(g),
            _ => new TextBlock { Text = Icon.ToString() },
        };
    }

    /// <inheritdoc/>
    protected override void OnAttachedToLogicalTree(Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);

        // A geometry resource key resolves only once the presenter can see the theme resources.
        if (Icon is string)
        {
            Rebuild();
        }
    }

    private Path MakePath(Geometry g)
    {
        var path = new Path { Data = g, Stretch = Stretch.Uniform };
        path.Bind(Shape.FillProperty, this.GetObservable(ForegroundProperty));
        return path;
    }
}
