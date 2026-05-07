using Avalonia.Media;
using ReactorDesigner.Models;

namespace ReactorDesigner.ViewModels;

public sealed class ToolboxItemViewModel : ViewModelBase
{
    public ToolboxItemViewModel(NodeTemplate template)
    {
        Template = template;
        AccentColor = Color.Parse(template.AccentHex);
        AccentBrush = new SolidColorBrush(AccentColor);
        ChipBrush = new SolidColorBrush(Color.FromArgb(45, AccentColor.R, AccentColor.G, AccentColor.B));
        IconBrush = new SolidColorBrush(Color.Parse("#F4F8FF"));
        IconGeometry = Geometry.Parse(template.IconPathData);
    }

    public NodeTemplate Template { get; }

    public NodeKind Kind => Template.Kind;

    public string Title => Template.Title;

    public string Description => Template.Description;

    public string Group => Template.Group;

    public Geometry IconGeometry { get; }

    public int InputCount => Template.InputPorts.Count;

    public int OutputCount => Template.OutputPorts.Count;

    public Color AccentColor { get; }

    public IBrush AccentBrush { get; }

    public IBrush ChipBrush { get; }

    public IBrush IconBrush { get; }
}
