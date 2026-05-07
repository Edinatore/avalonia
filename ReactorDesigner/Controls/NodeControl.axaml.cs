using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using ReactorDesigner.ViewModels;

namespace ReactorDesigner.Controls;

public partial class NodeControl : UserControl
{
    private const double DragThreshold = 4;

    private bool _isPointerDown;
    private bool _isDraggingNode;
    private Point _dragOffset;
    private Point _pressedCanvasPosition;

    public NodeControl()
    {
        InitializeComponent();
    }

    private void NodeRoot_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not NodeViewModel node ||
            sender is not InputElement inputElement)
        {
            return;
        }

        var canvas = this.FindAncestorOfType<GraphCanvas>();
        if (canvas is null)
        {
            return;
        }

        var currentPoint = e.GetCurrentPoint(this);
        if (currentPoint.Properties.IsRightButtonPressed)
        {
            node.Graph?.ShowNodeEditor(node, e.GetPosition(canvas));
            e.Handled = true;
            return;
        }

        if (!currentPoint.Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (e.Source is StyledElement { DataContext: PortViewModel })
        {
            return;
        }

        node.Graph?.CloseTransientPopups();
        node.Graph?.SelectNode(node);
        node.Graph?.BringNodeToFront(node);
        _isPointerDown = true;
        _isDraggingNode = false;
        _pressedCanvasPosition = e.GetPosition(canvas);
        _dragOffset = _pressedCanvasPosition - new Point(node.X, node.Y);
        e.Pointer.Capture(inputElement);
        e.Handled = true;
    }

    private void NodeRoot_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is not NodeViewModel node)
        {
            return;
        }

        var canvas = this.FindAncestorOfType<GraphCanvas>();
        if (canvas is null)
        {
            return;
        }

        if (!_isDraggingNode)
        {
            if (!_isPointerDown || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                ResetDragState();
                return;
            }

            var pendingPosition = e.GetPosition(canvas);
            if (Math.Abs(pendingPosition.X - _pressedCanvasPosition.X) < DragThreshold &&
                Math.Abs(pendingPosition.Y - _pressedCanvasPosition.Y) < DragThreshold)
            {
                return;
            }

            _isDraggingNode = true;
            node.Graph?.BringNodeToFront(node);
        }

        var pointerPosition = e.GetPosition(canvas);
        node.SetPosition(pointerPosition.X - _dragOffset.X, pointerPosition.Y - _dragOffset.Y);
        e.Handled = true;
    }

    private void NodeRoot_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        ResetDragState();
        e.Pointer.Capture(null);
    }

    private void NodeRoot_OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        ResetDragState();
    }

    private void OutputPort_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control { DataContext: PortViewModel port } ||
            !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var canvas = this.FindAncestorOfType<GraphCanvas>();
        if (canvas is null)
        {
            return;
        }

        canvas.StartConnectionPreview(port, e.Pointer, e.GetPosition(canvas));
        e.Handled = true;
    }

    private void ResetDragState()
    {
        _isPointerDown = false;
        _isDraggingNode = false;
    }
}
