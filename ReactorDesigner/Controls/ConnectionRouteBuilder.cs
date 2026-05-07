using Avalonia;
using ReactorDesigner.ViewModels;

namespace ReactorDesigner.Controls;

public static class ConnectionRouteBuilder
{
    public const double PortLeadLength = 22;
    public const double BendPointHitRadius = 14;
    public const double SegmentHitTolerance = 18;

    public static IReadOnlyList<Point> BuildPreviewPolyline(Point startAnchor, Point previewEnd)
    {
        var points = new List<Point>();
        AddPoint(points, startAnchor);

        var startLead = new Point(startAnchor.X + PortLeadLength, startAnchor.Y);
        AddPoint(points, startLead);
        AppendOrthogonalLeg(points, previewEnd);

        return points;
    }

    public static IReadOnlyList<ConnectionSegmentDescriptor> BuildSegments(ConnectionViewModel connection)
    {
        var points = new List<Point>();
        var segments = new List<ConnectionSegmentDescriptor>();

        var startAnchor = connection.SourcePort.GetAnchorPoint();
        var endAnchor = connection.TargetPort.GetAnchorPoint();
        var startLead = new Point(startAnchor.X + PortLeadLength, startAnchor.Y);
        var endLead = new Point(endAnchor.X - PortLeadLength, endAnchor.Y);

        AddPoint(points, startAnchor);
        AddSegment(points, segments, startLead, 0);

        for (var index = 0; index < connection.BendPoints.Count; index++)
        {
            AppendOrthogonalLeg(points, segments, connection.BendPoints[index].Position, index);
        }

        AppendOrthogonalLeg(points, segments, endLead, connection.BendPoints.Count);
        AddSegment(points, segments, endAnchor, connection.BendPoints.Count);

        return segments;
    }

    public static ConnectionSegmentHit? FindNearestSegment(IEnumerable<ConnectionViewModel> connections, Point position)
    {
        ConnectionSegmentHit? nearestHit = null;

        foreach (var connection in connections)
        {
            foreach (var segment in BuildSegments(connection))
            {
                var projectedPoint = ProjectPointOntoSegment(position, segment.Start, segment.End);
                var distance = GetDistance(position, projectedPoint);
                if (distance > SegmentHitTolerance)
                {
                    continue;
                }

                if (nearestHit is null || distance < nearestHit.Value.Distance)
                {
                    nearestHit = new ConnectionSegmentHit(connection, segment.InsertIndex, distance, projectedPoint);
                }
            }
        }

        return nearestHit;
    }

    public static ConnectionBendPointViewModel? FindNearestBendPoint(IEnumerable<ConnectionViewModel> connections, Point position)
    {
        ConnectionBendPointViewModel? nearestPoint = null;
        var nearestDistance = double.MaxValue;

        foreach (var bendPoint in connections.SelectMany(connection => connection.BendPoints))
        {
            var distance = GetDistance(position, bendPoint.Position);
            if (distance > BendPointHitRadius || distance >= nearestDistance)
            {
                continue;
            }

            nearestPoint = bendPoint;
            nearestDistance = distance;
        }

        return nearestPoint;
    }

    private static void AppendOrthogonalLeg(List<Point> points, Point target)
    {
        if (points.Count == 0)
        {
            AddPoint(points, target);
            return;
        }

        var current = points[^1];
        if (!AreClose(current.X, target.X) && !AreClose(current.Y, target.Y))
        {
            AddPoint(points, new Point(target.X, current.Y));
        }

        AddPoint(points, target);
    }

    private static void AppendOrthogonalLeg(
        List<Point> points,
        List<ConnectionSegmentDescriptor> segments,
        Point target,
        int insertIndex)
    {
        if (points.Count == 0)
        {
            AddPoint(points, target);
            return;
        }

        var current = points[^1];
        if (!AreClose(current.X, target.X) && !AreClose(current.Y, target.Y))
        {
            AddSegment(points, segments, new Point(target.X, current.Y), insertIndex);
        }

        AddSegment(points, segments, target, insertIndex);
    }

    private static void AddSegment(
        List<Point> points,
        List<ConnectionSegmentDescriptor> segments,
        Point nextPoint,
        int insertIndex)
    {
        var current = points[^1];
        if (AreSamePoint(current, nextPoint))
        {
            return;
        }

        points.Add(nextPoint);
        segments.Add(new ConnectionSegmentDescriptor(current, nextPoint, insertIndex));
    }

    private static void AddPoint(List<Point> points, Point point)
    {
        if (points.Count == 0 || !AreSamePoint(points[^1], point))
        {
            points.Add(point);
        }
    }

    private static Point ProjectPointOntoSegment(Point point, Point start, Point end)
    {
        var deltaX = end.X - start.X;
        var deltaY = end.Y - start.Y;

        if (AreClose(deltaX, 0) && AreClose(deltaY, 0))
        {
            return start;
        }

        var factor = ((point.X - start.X) * deltaX + (point.Y - start.Y) * deltaY) / ((deltaX * deltaX) + (deltaY * deltaY));
        factor = Math.Clamp(factor, 0, 1);

        return new Point(start.X + (deltaX * factor), start.Y + (deltaY * factor));
    }

    private static bool AreSamePoint(Point first, Point second)
    {
        return AreClose(first.X, second.X) && AreClose(first.Y, second.Y);
    }

    private static double GetDistance(Point first, Point second)
    {
        var deltaX = first.X - second.X;
        var deltaY = first.Y - second.Y;
        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
    }

    private static bool AreClose(double first, double second)
    {
        return Math.Abs(first - second) < 0.001;
    }

    public readonly record struct ConnectionSegmentDescriptor(Point Start, Point End, int InsertIndex);

    public readonly record struct ConnectionSegmentHit(
        ConnectionViewModel Connection,
        int InsertIndex,
        double Distance,
        Point InsertPosition);
}
