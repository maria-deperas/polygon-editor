using Avalonia.Controls;
using Avalonia.Interactivity;
using PolygonEditor.Model;

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
        if (BresenhamRadio.IsChecked == true)
        {
            Editor.UseBresenham = true;
        }
        else
        {
            Editor.UseBresenham = false;
        }
    }
}