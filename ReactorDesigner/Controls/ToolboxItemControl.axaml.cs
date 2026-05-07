using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using ReactorDesigner.ViewModels;

namespace ReactorDesigner.Controls;

public partial class ToolboxItemControl : UserControl
{
    private Point _dragStartPoint;
    private PointerPressedEventArgs? _dragTriggerEvent;
    private bool _isPointerDown;
    private bool _isDragging;

    public ToolboxItemControl()
    {
        InitializeComponent();
    }

    private void Root_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not ToolboxItemViewModel || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _isPointerDown = true;
        _dragTriggerEvent = e;
        _dragStartPoint = e.GetPosition(this);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    private async void Root_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isPointerDown || _isDragging || DataContext is not ToolboxItemViewModel itemViewModel)
        {
            return;
        }

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            ResetPointerState(e.Pointer);
            return;
        }

        var delta = e.GetPosition(this) - _dragStartPoint;
        if (Math.Abs(delta.X) < 6 && Math.Abs(delta.Y) < 6)
        {
            return;
        }

        _isDragging = true;

        var dataTransfer = new DataTransfer();
        var kindValue = itemViewModel.Kind.ToString();
        dataTransfer.Add(DataTransferItem.Create(GraphCanvas.ToolboxItemFormat, kindValue));
        dataTransfer.Add(DataTransferItem.CreateText(kindValue));

        if (_dragTriggerEvent is not null)
        {
            await DragDrop.DoDragDropAsync(_dragTriggerEvent, dataTransfer, DragDropEffects.Copy);
        }

        ResetPointerState(e.Pointer);
    }

    private void Root_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        ResetPointerState(e.Pointer);
    }

    private void Root_OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        ResetPointerState(e.Pointer);
    }

    private void Root_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not ToolboxItemViewModel itemViewModel)
        {
            return;
        }

        if (this.FindAncestorOfType<Window>()?.DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        mainWindowViewModel.Graph.CreateNodeFromToolbox(itemViewModel.Kind);
        e.Handled = true;
    }

    private void ResetPointerState(IPointer pointer)
    {
        _isPointerDown = false;
        _isDragging = false;
        _dragTriggerEvent = null;
        pointer.Capture(null);
    }
}
