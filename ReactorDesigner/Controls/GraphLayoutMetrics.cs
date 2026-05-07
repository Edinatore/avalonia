namespace ReactorDesigner.Controls;

public static class GraphLayoutMetrics
{
    public const double NodeWidth = 264;
    public const double HeaderHeight = 40;
    public const double PortRowHeight = 28;
    public const double PortSectionTop = 48;
    public const double PortAnchorStart = PortSectionTop + (PortRowHeight / 2);
    public const double NodeBottomPadding = 16;
    public const double MinNodeHeight = 168;

    public static double GetNodeHeight(int maxPortCount)
    {
        return Math.Max(MinNodeHeight, PortSectionTop + (maxPortCount * PortRowHeight) + NodeBottomPadding);
    }
}
