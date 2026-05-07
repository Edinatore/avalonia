namespace ReactorDesigner.Models;

public sealed class ConnectionModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SourcePortId { get; set; }

    public Guid TargetPortId { get; set; }

    public List<ConnectionBendPointModel> BendPoints { get; set; } = [];
}
