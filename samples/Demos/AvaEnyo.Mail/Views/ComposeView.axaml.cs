using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaEnyo.Controls;
using AvaEnyo.Mail.ViewModels;
using AvaEnyo.Samples;

namespace AvaEnyo.Mail.Views;

/// <summary>
/// The compose sheet: To and Cc token inputs with contact suggestions, a subject and a body. Send spins while the
/// service bus delivers the message.
/// </summary>
public partial class ComposeView : UserControl
{
    private readonly MailViewModel _mail;

    /// <summary>For the previewer: an empty sheet over the sample bus.</summary>
    public ComposeView()
        : this(new MailViewModel(SampleServiceBus.Create()), null)
    {
    }

    public ComposeView(MailViewModel mail, Draft? draft)
    {
        _mail = mail;
        InitializeComponent();
        var template = (IDataTemplate)Resources["ContactTemplate"]!;
        foreach (var input in new[] { ToInput, CcInput })
        {
            input.SuggestionsTemplate = template;
            input.FilterTextChanged += async (_, e) =>
            {
                var target = (TokenInput)e.Source!;
                target.Suggestions = string.IsNullOrWhiteSpace(target.FilterText) ? null : await _mail.SearchContactsAsync(target.FilterText);
            };
            input.Atomizing += (_, e) =>
            {
                // A typed name or address becomes the matching contact when there is one.
                var text = e.Text.Trim();
                e.Token = SampleData.Contacts.FirstOrDefault(c => c.Name.Equals(text, StringComparison.OrdinalIgnoreCase)
                    || c.Email.Equals(text, StringComparison.OrdinalIgnoreCase)) ?? (object)text;
            };
        }

        if (draft is not null)
        {
            Load(draft);
        }

        CancelButton.Click += (_, _) => Cancelled?.Invoke(this, EventArgs.Empty);
        SendButton.Click += async (_, _) =>
        {
            if (SendButton.IsActive)
            {
                return;
            }

            ToInput.Atomize();
            CcInput.Atomize();
            SendButton.IsActive = true;
            Fields.IsEnabled = false;
            BodyBox.IsEnabled = false;
            try
            {
                if (await _mail.SendAsync(Snapshot()))
                {
                    Sent?.Invoke(this, EventArgs.Empty);
                }
            }
            finally
            {
                SendButton.IsActive = false;
                Fields.IsEnabled = true;
                BodyBox.IsEnabled = true;
            }
        };
    }

    /// <summary>Raised after the bus confirmed the message was sent.</summary>
    public event EventHandler? Sent;

    /// <summary>Raised when Cancel is pressed.</summary>
    public event EventHandler? Cancelled;

    /// <summary>The fields as a draft.</summary>
    public Draft Snapshot() => new(ToInput.Tokens.ToList(), CcInput.Tokens.ToList(), SubjectBox.Text, BodyBox.Text);

    /// <summary>Fills the fields from a draft or a reply.</summary>
    public void Load(Draft draft)
    {
        ToInput.Tokens.Clear();
        ToInput.Tokens.AddRange(draft.To);
        CcInput.Tokens.Clear();
        CcInput.Tokens.AddRange(draft.Cc);
        SubjectBox.Text = draft.Subject;
        BodyBox.Text = draft.Body;
    }
}
