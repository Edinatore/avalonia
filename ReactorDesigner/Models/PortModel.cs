namespace ReactorDesigner.Models;

public sealed class PortModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Label { get; set; } = string.Empty;

    public PortDirection Direction { get; set; }

    public string ConnectionType { get; set; } = "ProcessStream";
}
