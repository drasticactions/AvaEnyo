namespace AvaEnyo.Samples;

/// <summary>A fake contact.</summary>
public sealed record Contact(int Id, string Name, string Email, int AvatarIndex)
{
    public string Initial => Name.Length > 0 ? Name[..1].ToUpperInvariant() : "#";

    public override string ToString() => Name;
}
