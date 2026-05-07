using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ReactorDesigner.ViewModels;

namespace ReactorDesigner.Controls;

public class ConnectionLayer : Control
{
    public static readonly StyledProperty<GraphViewModel?> GraphProperty =
        AvaloniaProperty.Register<ConnectionLayer, GraphViewModel?>(nameof(Graph));

    public ConnectionLayer()
    {
        IsHitTestVisible = false;
    }

    public GraphViewModel? Graph
    {
        get => GetValue(GraphProperty);
        set => SetValue(GraphProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Graph is null)
        {
            return;
        }

        foreach (var connection in Graph.Connections)
        {
            DrawConnection(context, connection);
        }

        if (Graph.PreviewSourcePort is not null)
        {
            var previewColor = Graph.PreviewSourcePort.Node.AccentColor;
            var previewBrush = new SolidColorBrush(Color.FromArgb(190, previewColor.R, previewColor.G, previewColor.B));

            DrawPolyline(
                context,
                ConnectionRouteBuilder.BuildPreviewPolyline(
                    Graph.PreviewSourcePort.GetAnchorPoint(),
                    Graph.PreviewTargetPoint),
                previewBrush,
                2.5,
                null,
                null);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == GraphProperty)
        {
            var (oldGraph, newGraph) = change.GetOldAndNewValue<GraphViewModel?>();
            OnGraphChanged(oldGraph, newGraph);
        }
    }

    private void OnGraphChanged(GraphViewModel? oldGraph, GraphViewModel? newGraph)
    {
        if (oldGraph is not null)
        {
            oldGraph.VisualRefreshRequested -= HandleGraphVisualRefreshRequested;
        }

        if (newGraph is not null)
        {
            newGraph.VisualRefreshRequested += HandleGraphVisualRefreshRequested;
        }

        InvalidateVisual();
    }

    private void HandleGraphVisualRefreshRequested(object? sender, EventArgs e)
    {
        InvalidateVisual();
    }

    private static void DrawConnection(DrawingContext context, ConnectionViewModel connection)
    {
        var segments = ConnectionRouteBuilder.BuildSegments(connection);
        var points = new List<Point>();

        foreach (var segment in segments)
        {
            if (points.Count == 0)
            {
                points.Add(segment.Start);
            }

            points.Add(segment.End);
        }

        DrawPolyline(
            context,
            points,
            connection.StrokeBrush,
            connection.Thickness,
            connection.HandleFillBrush,
            connection.HandleStrokeBrush);

        foreach (var bendPoint in connection.BendPoints)
        {
            var handleRect = new Rect(bendPoint.X - 5, bendPoint.Y - 5, 10, 10);
            context.FillRectangle(connection.HandleFillBrush, handleRect);
            context.DrawRectangle(new Pen(connection.HandleStrokeBrush, 1.5), handleRect);
        }
    }

    private static void DrawPolyline(
        DrawingContext context,
        IReadOnlyList<Point> points,
        IBrush strokeBrush,
        double thickness,
        IBrush? handleFillBrush,
        IBrush? handleStrokeBrush)
    {
        if (points.Count < 2)
        {
            return;
        }

        var color = strokeBrush switch
        {
            ISolidColorBrush solidColorBrush => solidColorBrush.Color,
            _ => Color.Parse("#8EA4B8")
        };

        var glowPen = new Pen(new SolidColorBrush(Color.FromArgb(44, color.R, color.G, color.B)), thickness + 6);
        var mainPen = new Pen(strokeBrush, thickness);

        for (var index = 0; index < points.Count - 1; index++)
        {
            context.DrawLine(glowPen, points[index], points[index + 1]);
            context.DrawLine(mainPen, points[index], points[index + 1]);
        }

        DrawArrowHead(context, points, strokeBrush);
    }

    private static void DrawArrowHead(DrawingContext context, IReadOnlyList<Point> points, IBrush strokeBrush)
    {
        for (var index = points.Count - 1; index > 0; index--)
        {
            var tip = points[index];
            var previous = points[index - 1];
            var directionX = tip.X - previous.X;
            var directionY = tip.Y - previous.Y;

            if (Math.Abs(directionX) < 0.001 && Math.Abs(directionY) < 0.001)
            {
                continue;
            }

            var length = Math.Sqrt((directionX * directionX) + (directionY * directionY));
            var unitX = directionX / length;
            var unitY = directionY / length;
            var perpendicularX = -unitY;
            var perpendicularY = unitX;

            const double arrowLength = 12;
            const double arrowHalfWidth = 5;

            var baseCenter = new Point(
                tip.X - (unitX * arrowLength),
                tip.Y - (unitY * arrowLength));

            var left = new Point(
                baseCenter.X + (perpendicularX * arrowHalfWidth),
                baseCenter.Y + (perpendicularY * arrowHalfWidth));

            var right = new Point(
                baseCenter.X - (perpendicularX * arrowHalfWidth),
                baseCenter.Y - (perpendicularY * arrowHalfWidth));

            var geometry = new StreamGeometry();
            using var geometryContext = geometry.Open();
            geometryContext.BeginFigure(tip, true);
            geometryContext.LineTo(left);
            geometryContext.LineTo(right);
            geometryContext.EndFigure(true);

            context.DrawGeometry(strokeBrush, null, geometry);
            return;
        }
    }
}
