using System;
using System.Linq;
using Avalonia.Controls;
using AvaEnyo.Samples;

namespace AvaEnyo.Gallery.Pages.Widgets;

public partial class TokenInputPage : UserControl
{
    public TokenInputPage()
    {
        InitializeComponent();
        Tokens.Tokens.Add(SampleData.Contacts[6].Name);
        Tokens.FilterTextChanged += (_, e) =>
        {
            // Autocomplete from the fake contacts: names that start with, or contain, the typed text.
            Tokens.Suggestions = string.IsNullOrWhiteSpace(e.Text)
                ? null
                : SampleData.Contacts
                    .Where(c => c.Name.Contains(e.Text, StringComparison.CurrentCultureIgnoreCase))
                    .Select(c => c.Name)
                    .Take(6)
                    .ToList();
        };
    }
}
