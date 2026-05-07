using System.Collections.ObjectModel;

namespace ReactorDesigner.Models;

public sealed class NodeTemplate
{
    public NodeTemplate(
        NodeKind kind,
        string group,
        string title,
        string description,
        string accentHex,
        string iconPathData,
        IEnumerable<PortTemplate> inputPorts,
        IEnumerable<PortTemplate> outputPorts)
    {
        Kind = kind;
        Group = group;
        Title = title;
        Description = description;
        AccentHex = accentHex;
        IconPathData = iconPathData;
        InputPorts = new ReadOnlyCollection<PortTemplate>(inputPorts.ToList());
        OutputPorts = new ReadOnlyCollection<PortTemplate>(outputPorts.ToList());
    }

    public NodeKind Kind { get; }

    public string Group { get; }

    public string Title { get; }

    public string Description { get; }

    public string AccentHex { get; }

    public string IconPathData { get; }

    public ReadOnlyCollection<PortTemplate> InputPorts { get; }

    public ReadOnlyCollection<PortTemplate> OutputPorts { get; }
}
