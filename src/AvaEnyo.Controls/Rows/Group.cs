using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// The Enyo group (<c>enyo.Group</c>): a rounded, embossed 14 px frame around a stack of rows. With a
/// <see cref="HeaderedItemsControl.Header"/> a 36 px dark band on top carries the uppercase white label. The
/// children sit in an <see cref="OrderedPanel"/>, so the first and last <see cref="Item"/> get rounded corners.
/// </summary>
[PseudoClasses(":labeled", ":content-fit", ":text-header")]
public class Group : HeaderedItemsControl
{
    /// <summary>Defines the <see cref="ContentFit"/> property.</summary>
    public static readonly StyledProperty<bool> ContentFitProperty =
        AvaloniaProperty.Register<Group, bool>(nameof(ContentFit));

    /// <summary>Defines the <see cref="HeaderText"/> property.</summary>
    public static readonly DirectProperty<Group, string?> HeaderTextProperty =
        AvaloniaProperty.RegisterDirect<Group, string?>(nameof(HeaderText), o => o.HeaderText);

    private static readonly FuncTemplate<Panel?> s_panel = new(() => new OrderedPanel());

    private string? _headerText;

    static Group()
    {
        ItemsPanelProperty.OverrideDefaultValue<Group>(s_panel);
        HeaderProperty.Changed.AddClassHandler<Group>((g, _) => g.UpdatePseudoClasses());
        ContentFitProperty.Changed.AddClassHandler<Group>((g, _) => g.UpdatePseudoClasses());
    }

    /// <summary>Initializes a new instance of the <see cref="Group"/> class.</summary>
    public Group()
    {
        UpdatePseudoClasses();
    }

    /// <summary>
    /// True when the group has an explicit size and its content should stretch to fill it (Enyo <c>contentFit</c>).
    /// </summary>
    public bool ContentFit
    {
        get => GetValue(ContentFitProperty);
        set => SetValue(ContentFitProperty, value);
    }

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<Control>(item, out recycleKey);

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new Item();

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

        // .enyo-group-inner > .enyo-item: rows in a group are rounded by position.
        if (container is Item row)
        {
            row.Classes.Set("rounded", true);
        }
    }

    /// <summary>
    /// The header when it is plain text, or null. The template shows it as the uppercase white label with the
    /// #646454 text shadow of <c>.enyo-group-label</c>; any other header is presented as content.
    /// </summary>
    public string? HeaderText
    {
        get => _headerText;
        private set => SetAndRaise(HeaderTextProperty, ref _headerText, value);
    }

    private void UpdatePseudoClasses()
    {
        HeaderText = Header as string;
        PseudoClasses.Set(":text-header", Header is string);
        PseudoClasses.Set(":labeled", Header is not null);
        PseudoClasses.Set(":content-fit", ContentFit);
    }
}
