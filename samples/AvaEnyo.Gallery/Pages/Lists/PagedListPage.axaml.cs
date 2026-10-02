using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaEnyo.Controls;

namespace AvaEnyo.Gallery.Pages.Lists;

public partial class PagedListPage : UserControl
{
    public PagedListPage()
    {
        InitializeComponent();

        // 1,000 rows, loaded ten at a time with a fake 300 ms delay; unloaded rows show a placeholder.
        List.ItemsSource = new PagedCollection<string>(1000, Load, pageSize: 10);
        List.ItemTemplate = new FuncDataTemplate<string?>((s, _) => new TextBlock { Text = s ?? "Loading…", Opacity = s is null ? 0.5 : 1 });
        List.AcquirePage += (_, e) => Write($"acquire page {e.Page}");
        List.DiscardPage += (_, e) => Write($"discard page {e.Page}");
    }

    private static async Task<IReadOnlyList<string>> Load(int page, CancellationToken token)
    {
        await Task.Delay(300, token);
        var rows = new List<string>(10);
        for (var i = 0; i < 10; i++)
        {
            rows.Add($"Row {(page * 10) + i + 1} (page {page})");
        }

        return rows;
    }

    private void Write(string line) => Log.Text = line + Environment.NewLine + Log.Text;
}
