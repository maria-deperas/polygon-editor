using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace PolygonEditor.Views;

/// <summary>Modal dialog asking for an edge length; returns null when cancelled.</summary>
public sealed class LengthDialog : Window
{
    private readonly TextBox _input;

    public LengthDialog(double currentLength)
    {
        Title = "Set edge length";
        Width = 260;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        _input = new TextBox { Text = currentLength.ToString("0.##") };
        var ok = new Button { Content = "OK", IsDefault = true };
        var cancel = new Button { Content = "Cancel", IsCancel = true };
        ok.Click += (_, _) => Confirm();
        cancel.Click += (_, _) => Close(null);

        Content = new StackPanel
        {
            Margin = new Thickness(12),
            Spacing = 8,
            Children =
            {
                new TextBlock { Text = "Length:" },
                _input,
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Spacing = 8,
                    Children = { ok, cancel }
                }
            }
        };
    }

    private void Confirm()
    {
        if (double.TryParse(_input.Text, out double length) && length > 0)
            Close(length);
    }
}