using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ReactorDesigner.Models;

namespace ReactorDesigner.Views;

public partial class MainWindow : Window
{
    private NodeModel? _draggedNode;
    private Point _lastMousePosition;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Node_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is NodeModel node)
        {
            _draggedNode = node;
            _lastMousePosition = e.GetPosition(this);
        }
    }

    private void Node_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_draggedNode == null)
            return;

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var currentPosition = e.GetPosition(this);

            var delta = currentPosition - _lastMousePosition;

            _draggedNode.X += delta.X;
            _draggedNode.Y += delta.Y;

            _lastMousePosition = currentPosition;
        }
    }

    private void Node_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _draggedNode = null;
    }
}