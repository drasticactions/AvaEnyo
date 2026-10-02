namespace AvaEnyo.Controls;

/// <summary>The key layout of a <see cref="NumericKeypad"/>.</summary>
public enum NumericKeypadMode
{
    /// <summary>A phone dial pad: 1–9, <c>*</c>, 0, <c>#</c>, with letters under the digits.</summary>
    Phone,

    /// <summary>A PIN pad: 1–9, an empty key, 0, backspace.</summary>
    Pin,
}
