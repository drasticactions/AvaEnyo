namespace AvaEnyo;

/// <summary>How a <c>ScrollViewer</c> responds to drags.</summary>
public enum ScrollPhysicsMode
{
    /// <summary>Enyo's scroller: 1:1 drag, half-strength overscroll, friction fling and spring-back.</summary>
    Enyo,

    /// <summary>The stock presenter behavior: no drag scrolling from AvaEnyo.</summary>
    Native,
}
