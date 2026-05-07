namespace ReactorDesigner.Models;

public sealed class PortTemplate
{
    public PortTemplate(string label, PortDirection direction, string connectionType)
    {
        Label = label;
        Direction = direction;
        ConnectionType = connectionType;
    }

    public string Label { get; }

    public PortDirection Direction { get; }

    public string ConnectionType { get; }
}
