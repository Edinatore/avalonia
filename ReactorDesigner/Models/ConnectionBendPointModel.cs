namespace ReactorDesigner.Models;

public sealed class ConnectionBendPointModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public double X { get; set; }

    public double Y { get; set; }
}
