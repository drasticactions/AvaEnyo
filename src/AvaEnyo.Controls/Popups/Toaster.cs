using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.VisualTree;
using AvaEnyo.Animations;
using AvaEnyo.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// A popup that slides in from a window edge (<c>enyo.Toaster</c>): over 350 ms with a cubic ease-out from
/// <see cref="FlyInFrom"/> (the bottom by default), against that edge. Dragging a sliding handle inside
/// (<c>enyo:Sliding.IsHandle="True"</c>) moves it toward the edge; on release it closes if the last motion was toward
/// the edge, and slides back otherwise.
/// </summary>
public class Toaster : PopupBase
{
    /// <summary>Defines the <see cref="FlyInFrom"/> property.</summary>
    public static readonly StyledProperty<ToasterEdge> FlyInFromProperty =
        AvaloniaProperty.Register<Toaster, ToasterEdge>(nameof(FlyInFrom));

    private DragTracker? _drag;
    private double _dragOffset;
    private double _lastStep;

    /// <summary>The edge the toaster flies in from and rests against.</summary>
    public ToasterEdge FlyInFrom
    {
        get => GetValue(FlyInFromProperty);
        set => SetValue(FlyInFromProperty, value);
    }

    private bool Horizontal => FlyInFrom is ToasterEdge.Left or ToasterEdge.Right;

    private int Outward => FlyInFrom is ToasterEdge.Bottom or ToasterEdge.Right ? 1 : -1;

    /// <inheritdoc/>
    protected override PopupPresenter CreatePresenter() => new ToasterPresenter();

    /// <inheritdoc/>
    protected override Point? PlacePresenter(Size size, Rect viewport) => FlyInFrom switch
    {
        ToasterEdge.Top => new Point(viewport.Left, viewport.Top),
        ToasterEdge.Left => new Point(viewport.Left, viewport.Top),
        ToasterEdge.Right => new Point(viewport.Right - size.Width, viewport.Top),
        _ => new Point(viewport.Left, viewport.Bottom - size.Height),
    };

    /// <inheritdoc/>
    protected override void OnPresenterAttached(PopupPresenter presenter)
    {
        base.OnPresenterAttached(presenter);
        presenter.Bind(ToasterPresenter.EdgeProperty, this.GetObservable(FlyInFromProperty));
        _drag?.Dispose();
        _drag = new DragTracker(presenter) { CanStart = CanStartDrag };
        _drag.Started += (_, e) => OnDragMoved(e);
        _drag.Moved += (_, e) => OnDragMoved(e);
        _drag.Completed += (_, _) => OnDragFinished();
        _drag.Canceled += (_, _) => OnDragFinished();
    }

    /// <inheritdoc/>
    protected override Task AnimateOpenAsync(PopupPresenter presenter) =>
        EnyoAnimations.ToasterIn(presenter, FlyInFrom, Distance(presenter));

    /// <inheritdoc/>
    protected override async Task AnimateCloseAsync(PopupPresenter presenter)
    {
        _drag?.Dispose();
        _drag = null;
        await EnyoAnimations.ToasterOut(presenter, FlyInFrom, Distance(presenter)).ConfigureAwait(true);
    }

    private double Distance(Control presenter) => Horizontal ? presenter.Bounds.Width : presenter.Bounds.Height;

    private bool CanStartDrag(DragStartInfo info)
    {
        if (info.IsHorizontal != Horizontal || Presenter is not { } presenter)
        {
            return false;
        }

        for (var v = info.Source as Visual; v is not null && v != presenter; v = v.GetVisualParent())
        {
            if (v is Control c && Sliding.GetIsHandle(c) is { } handle)
            {
                if (handle)
                {
                    _dragOffset = Horizontal ? info.Delta.X : info.Delta.Y;
                    _lastStep = 0;
                }

                return handle;
            }
        }

        return false;
    }

    private void OnDragMoved(DragTrackerEventArgs e)
    {
        if (Presenter is not { } presenter)
        {
            return;
        }

        var d = (Horizontal ? e.Delta.X : e.Delta.Y) - _dragOffset;
        var step = Horizontal ? e.Step.X : e.Step.Y;
        if (step != 0)
        {
            _lastStep = step;
        }

        // Only motion toward the edge moves the toaster (Enyo dragHandler).
        var shift = d * Outward > 0 ? d : 0;
        var b = TransformOperations.CreateBuilder(1);
        b.AppendTranslate(Horizontal ? shift : 0, Horizontal ? 0 : shift);
        presenter.RenderTransform = b.Build();
    }

    private void OnDragFinished()
    {
        if (Presenter is not { } presenter)
        {
            return;
        }

        if (_lastStep * Outward > 0)
        {
            CloseCore(PopupCloseReason.Code, null);
        }
        else
        {
            _ = EnyoAnimations.ToasterIn(presenter, FlyInFrom, Distance(presenter));
        }
    }
}
