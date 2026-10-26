using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Markdown.Avalonia;

namespace PolygonEditor.Views;

/// <summary>Shows the documentation compiled into the program (Docs/Documentation.md).</summary>
public sealed class HelpWindow : Window
{
    public HelpWindow()
    {
        Title = "Documentation";
        Width = 760;
        Height = 600;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        Content = new MarkdownScrollViewer
        {
            Markdown = LoadText(),
            Margin = new Thickness(16),
        };
    }

    private static string LoadText()
    {
        using Stream? stream = typeof(HelpWindow).Assembly.GetManifestResourceStream("Documentation.md");
        if (stream == null) return "Documentation not found.";
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}