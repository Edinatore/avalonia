using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using ReactorDesigner.Models;
using ReactorDesigner.ViewModels;

namespace ReactorDesigner.Controls;

public class GraphCanvas : Grid
{
    public const string ToolboxItemFormatIdentifier = "reactordesigner.node-kind";

    public static readonly DataFormat<string> ToolboxItemFormat =
        DataFormat.CreateStringApplicationFormat(ToolboxItemFormatIdentifier);

    public static readonly StyledProperty<GraphViewModel?> GraphProperty =
        AvaloniaProperty.Register<GraphCanvas, GraphViewModel?>(nameof(Graph));

    public GraphCanvas()
    {
        DragDrop.SetAllowDrop(this, true);
        ClipToBounds = true;
        Children.Insert(0, new GridBackgroundLayer());

        AddHandler(DragDrop.DragOverEvent, HandleDragOver);
        AddHandler(DragDrop.DropEvent, HandleDrop);

        PointerPressed += HandlePointerPressed;
        PointerMoved += HandlePointerMoved;
        PointerReleased += HandlePointerReleased;
        PointerCaptureLost += HandlePointerCaptureLost;
    }

    public GraphViewModel? Graph
    {
        get => GetValue(GraphProperty);
        set => SetValue(GraphProperty, value);
    }

    public void StartConnectionPreview(PortViewModel sourcePort, IPointer pointer, Point currentPosition)
    {
        Graph?.BeginConnectionPreview(sourcePort, currentPosition);
        pointer.Capture(this);
    }

    private void HandlePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Graph is null || Graph.IsConnectionPreviewActive)
        {
            return;
        }

        if (IsWithinPopupSurface(e.Source as Visual))
        {
            return;
        }

        var position = e.GetPosition(this);
        var currentPoint = e.GetCurrentPoint(this);

        if (currentPoint.Properties.IsRightButtonPressed)
        {
            if (Graph.TryShowConnectionMenu(position))
            {
                e.Handled = true;
                return;
            }

            Graph.ShowCanvasAddMenu(position);
            e.Handled = true;
            return;
        }

        if (!currentPoint.Properties.IsLeftButtonPressed)
        {
            return;
        }

        Graph.CloseTransientPopups();

        if (e.ClickCount >= 2 && Graph.TryInsertBendPoint(position))
        {
            e.Pointer.Capture(this);
            e.Handled = true;
            return;
        }

        if (Graph.TryBeginBendPointDrag(position))
        {
            e.Pointer.Capture(this);
            e.Handled = true;
            return;
        }

        Graph.SelectNode(null);
    }

    private void HandleDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = TryGetNodeKind(e.DataTransfer, out _)
            ? DragDropEffects.Copy
            : DragDropEffects.None;

        e.Handled = true;
    }

    private void HandleDrop(object? sender, DragEventArgs e)
    {
        if (Graph is null || !TryGetNodeKind(e.DataTransfer, out var kind))
        {
            return;
        }

        Graph.CreateNode(kind, e.GetPosition(this));
        e.Handled = true;
    }

    private void HandlePointerMoved(object? sender, PointerEventArgs e)
    {
        if (Graph?.IsBendPointDragActive == true)
        {
            Graph.UpdateBendPointDrag(e.GetPosition(this));
            e.Handled = true;
            return;
        }

        if (Graph?.IsConnectionPreviewActive == true)
        {
            Graph.UpdateConnectionPreview(e.GetPosition(this));
            e.Handled = true;
        }
    }

    private void HandlePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (Graph?.IsBendPointDragActive == true)
        {
            Graph.EndBendPointDrag();
            e.Pointer.Capture(null);
            e.Handled = true;
            return;
        }

        if (Graph?.IsConnectionPreviewActive != true)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        PortViewModel? targetPort = null;

        if (topLevel is not null)
        {
            var hit = topLevel.InputHitTest(e.GetPosition(topLevel)) as Visual;
            targetPort = FindDataContext<PortViewModel>(hit);
        }

        Graph.TryCompleteConnection(targetPort);
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void HandlePointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (Graph?.IsBendPointDragActive == true)
        {
            Graph.EndBendPointDrag();
        }

        if (Graph?.IsConnectionPreviewActive == true)
        {
            Graph.CancelConnectionPreview();
        }
    }

    private static bool TryGetNodeKind(IDataTransfer dataTransfer, out NodeKind kind)
    {
        kind = default;

        if (dataTransfer.TryGetValue(ToolboxItemFormat) is string rawValue &&
            Enum.TryParse(rawValue, out kind))
        {
            return true;
        }

        if (dataTransfer.TryGetText() is string textValue &&
            Enum.TryParse(textValue, out kind))
        {
            return true;
        }

        return false;
    }

    private static T? FindDataContext<T>(Visual? visual)
        where T : class
    {
        while (visual is not null)
        {
            if (visual is StyledElement { DataContext: T context })
            {
                return context;
            }

            visual = visual.GetVisualParent();
        }

        return null;
    }

    private static bool IsWithinPopupSurface(Visual? visual)
    {
        while (visual is not null)
        {
            if (visual is StyledElement styledElement &&
                styledElement.Classes.Contains("graph-popup-surface"))
            {
                return true;
            }

            visual = visual.GetVisualParent();
        }

        return false;
    }

    private sealed class GridBackgroundLayer : Control
    {
        private static readonly SolidColorBrush SurfaceBrush = new(Color.Parse("#10161E"));
        private static readonly Pen MinorGridPen = new(new SolidColorBrush(Color.Parse("#1D2734")), 1);
        private static readonly Pen MajorGridPen = new(new SolidColorBrush(Color.Parse("#2B3848")), 1);

        public GridBackgroundLayer()
        {
            IsHitTestVisible = false;
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            var bounds = Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            context.FillRectangle(SurfaceBrush, bounds);

            const double minorStep = 24;
            const double majorStep = 120;

            for (var x = 0.0; x <= bounds.Width; x += minorStep)
            {
                var pen = Math.Abs(x % majorStep) < 0.1 ? MajorGridPen : MinorGridPen;
                context.DrawLine(pen, new Point(x, 0), new Point(x, bounds.Height));
            }

            for (var y = 0.0; y <= bounds.Height; y += minorStep)
            {
                var pen = Math.Abs(y % majorStep) < 0.1 ? MajorGridPen : MinorGridPen;
                context.DrawLine(pen, new Point(0, y), new Point(bounds.Width, y));
            }
        }
    }
}
