using Avalonia.Controls;
using Avalonia.Interactivity;
using PolygonEditor.Model;
using PolygonEditor.Views;

namespace PolygonEditor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Editor.Scene = DemoScene.Create();
    }
    
    private void OnBresenhamChanged(object? sender, RoutedEventArgs e)
    {
        Editor.UseBresenham = BresenhamRadio.IsChecked == true;
    }

    private void OnHelpClick(object? sender, RoutedEventArgs e) => ShowHelp();

    private void ShowHelp() => new HelpWindow().Show(this);
}