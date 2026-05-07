using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactorDesigner.Models;

namespace ReactorDesigner.ViewModels;

public partial class PortViewModel : ViewModelBase
{
    public PortViewModel(PortModel model, NodeViewModel node, int index)
    {
        Model = model;
        Node = node;
        this.index = index;
        label = model.Label;

        fillBrush = Brushes.Transparent;
        strokeBrush = Brushes.Transparent;
        UpdateBrushes(node.AccentColor);
    }

    public PortModel Model { get; }

    public Guid Id => Model.Id;

    [ObservableProperty]
    private string label;

    public PortDirection Direction => Model.Direction;

    public string ConnectionType => Model.ConnectionType;

    [ObservableProperty]
    private int index;

    public NodeViewModel Node { get; }

    [ObservableProperty]
    private IBrush fillBrush;

    [ObservableProperty]
    private IBrush strokeBrush;

    public Point GetAnchorPoint()
    {
        return Node.GetPortAnchorPoint(this);
    }

    public void UpdateBrushes(Color accentColor)
    {
        FillBrush = new SolidColorBrush(Color.FromArgb(255, accentColor.R, accentColor.G, accentColor.B));
        StrokeBrush = new SolidColorBrush(Color.FromArgb(255, 245, 249, 255));
    }

    partial void OnLabelChanged(string value)
    {
        Model.Label = value;
    }
}
