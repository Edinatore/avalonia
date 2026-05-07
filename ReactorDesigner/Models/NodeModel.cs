using System.Collections.ObjectModel;

namespace ReactorDesigner.Models;

public sealed class NodeModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public NodeKind Kind { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string AccentHex { get; set; } = "#4D7FAE";

    public string IconPathData { get; set; } = string.Empty;

    public ObservableCollection<PortModel> InputPorts { get; } = new();

    public ObservableCollection<PortModel> OutputPorts { get; } = new();
}
