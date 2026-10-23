using Avalonia.Controls;
using PolygonEditor.Model;

namespace PolygonEditor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Editor.Scene = DemoScene.Create();
    }
}