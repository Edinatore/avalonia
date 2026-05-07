using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactorDesigner.Controls;
using ReactorDesigner.Models;
using System.Collections.ObjectModel;

namespace ReactorDesigner.ViewModels;

public partial class NodeViewModel : ViewModelBase
{
    public NodeViewModel(NodeModel model)
    {
        Model = model;
        x = model.X;
        y = model.Y;
        title = model.Title;
        accentHex = model.AccentHex;

        CategoryLabel = model.Kind switch
        {
            NodeKind.Custom => "Custom Node",
            NodeKind.Reactor => "Process Vessel",
            NodeKind.Pump => "Hydraulic Drive",
            NodeKind.HeatExchanger => "Thermal Unit",
            NodeKind.Valve => "Flow Control",
            NodeKind.PipeJunction => "Split Junction",
            NodeKind.Turbine => "Power Stage",
            NodeKind.Sensor => "Instrumentation",
            _ => "Process Node"
        };
        BadgeText = model.Kind switch
        {
            NodeKind.Custom => "CU",
            NodeKind.Reactor => "RX",
            NodeKind.Pump => "PU",
            NodeKind.HeatExchanger => "HX",
            NodeKind.Valve => "VL",
            NodeKind.PipeJunction => "JN",
            NodeKind.Turbine => "TB",
            NodeKind.Sensor => "SN",
            _ => "ND"
        };
        Description = model.Description;
        IconGeometry = Geometry.Parse(model.IconPathData);

        bodyBrush = new SolidColorBrush(Color.Parse("#18212C"));
        headerBrush = new SolidColorBrush(Color.Parse("#4B6A88"));
        outlineBrush = new SolidColorBrush(Color.Parse("#4B6A88"));
        iconPlateBrush = new SolidColorBrush(Color.FromArgb(255, 27, 38, 50));
        IconBrush = new SolidColorBrush(Color.Parse("#F4F8FF"));
        SubtleTextBrush = new SolidColorBrush(Color.Parse("#9FB0C3"));

        InputPorts = new ObservableCollection<PortViewModel>(model.InputPorts.Select((port, index) => new PortViewModel(port, this, index)));
        OutputPorts = new ObservableCollection<PortViewModel>(model.OutputPorts.Select((port, index) => new PortViewModel(port, this, index)));

        ApplyAccentColor(accentHex);
    }

    public NodeModel Model { get; }

    public Guid Id => Model.Id;

    public NodeKind Kind => Model.Kind;

    public string CategoryLabel { get; }

    public string BadgeText { get; }

    public string Description { get; }

    public Geometry IconGeometry { get; }

    public Color AccentColor { get; private set; }

    public IBrush BodyBrush
    {
        get => bodyBrush;
        private set => SetProperty(ref bodyBrush, value);
    }

    public IBrush HeaderBrush
    {
        get => headerBrush;
        private set => SetProperty(ref headerBrush, value);
    }

    public IBrush OutlineBrush
    {
        get => outlineBrush;
        private set => SetProperty(ref outlineBrush, value);
    }

    public IBrush IconPlateBrush
    {
        get => iconPlateBrush;
        private set => SetProperty(ref iconPlateBrush, value);
    }

    public IBrush IconBrush { get; }

    public IBrush SubtleTextBrush { get; }

    public double Width => GraphLayoutMetrics.NodeWidth;

    public double Height => GraphLayoutMetrics.GetNodeHeight(Math.Max(InputPorts.Count, OutputPorts.Count));

    public GraphViewModel? Graph { get; private set; }

    public ObservableCollection<PortViewModel> InputPorts { get; }

    public ObservableCollection<PortViewModel> OutputPorts { get; }

    public int InputCount
    {
        get => InputPorts.Count;
        set => ResizePorts(PortDirection.Input, value);
    }

    public int OutputCount
    {
        get => OutputPorts.Count;
        set => ResizePorts(PortDirection.Output, value);
    }

    [ObservableProperty]
    private double x;

    [ObservableProperty]
    private double y;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string accentHex;

    [ObservableProperty]
    private bool isSelected;

    private IBrush bodyBrush;
    private IBrush headerBrush;
    private IBrush outlineBrush;
    private IBrush iconPlateBrush;

    public void AttachToGraph(GraphViewModel graph)
    {
        Graph = graph;
    }

    public void RefreshPortNames()
    {
        ReindexPorts(InputPorts);
        ReindexPorts(OutputPorts);
    }

    public void SetPosition(double newX, double newY)
    {
        var maxX = Math.Max(0, (Graph?.WorkspaceWidth ?? Width) - Width);
        var maxY = Math.Max(0, (Graph?.WorkspaceHeight ?? Height) - Height);

        X = Math.Clamp(newX, 0, maxX);
        Y = Math.Clamp(newY, 0, maxY);

        Model.X = X;
        Model.Y = Y;
    }

    public Point GetPortAnchorPoint(PortViewModel port)
    {
        var anchorY = Y + GraphLayoutMetrics.PortAnchorStart + (port.Index * GraphLayoutMetrics.PortRowHeight);
        var anchorX = port.Direction == PortDirection.Input ? X : X + Width;
        return new Point(anchorX, anchorY);
    }

    partial void OnXChanged(double value)
    {
        Model.X = value;
        Graph?.RequestVisualRefresh();
    }

    partial void OnYChanged(double value)
    {
        Model.Y = value;
        Graph?.RequestVisualRefresh();
    }

    partial void OnTitleChanged(string value)
    {
        Model.Title = value;
    }

    partial void OnAccentHexChanged(string value)
    {
        Model.AccentHex = value;
        ApplyAccentColor(value);
    }

    partial void OnIsSelectedChanged(bool value)
    {
        BodyBrush = new SolidColorBrush(value ? Color.Parse("#1E2935") : Color.Parse("#18212C"));
        IconPlateBrush = new SolidColorBrush(value ? Color.FromArgb(255, 34, 46, 60) : Color.FromArgb(255, 27, 38, 50));
        OutlineBrush = new SolidColorBrush(Color.FromArgb(value ? (byte)255 : (byte)225, AccentColor.R, AccentColor.G, AccentColor.B));
        Graph?.RequestVisualRefresh();
    }

    private void ResizePorts(PortDirection direction, int targetCount)
    {
        targetCount = Math.Clamp(targetCount, 0, 16);

        var ports = direction == PortDirection.Input ? InputPorts : OutputPorts;
        var models = direction == PortDirection.Input ? Model.InputPorts : Model.OutputPorts;
        var removedPortIds = new List<Guid>();

        while (ports.Count < targetCount)
        {
            var label = direction == PortDirection.Input
                ? $"In {ports.Count + 1}"
                : $"Out {ports.Count + 1}";

            var portModel = new PortModel
            {
                Label = label,
                Direction = direction,
                ConnectionType = "ProcessStream"
            };

            models.Add(portModel);

            var portViewModel = new PortViewModel(portModel, this, ports.Count);
            ports.Add(portViewModel);
        }

        while (ports.Count > targetCount)
        {
            var removedPort = ports[^1];
            removedPortIds.Add(removedPort.Id);
            ports.RemoveAt(ports.Count - 1);
            models.RemoveAt(models.Count - 1);
        }

        ReindexPorts(ports);
        OnPropertyChanged(direction == PortDirection.Input ? nameof(InputCount) : nameof(OutputCount));
        Graph?.HandlePortTopologyChanged(this, removedPortIds);
    }

    private void ReindexPorts(ObservableCollection<PortViewModel> ports)
    {
        for (var index = 0; index < ports.Count; index++)
        {
            ports[index].Index = index;
        }
    }

    private void ApplyAccentColor(string value)
    {
        try
        {
            AccentColor = Color.Parse(value);
        }
        catch
        {
            AccentColor = Color.Parse("#4B6A88");
        }

        HeaderBrush = new SolidColorBrush(Color.FromArgb(255, AccentColor.R, AccentColor.G, AccentColor.B));
        OutlineBrush = new SolidColorBrush(Color.FromArgb(IsSelected ? (byte)255 : (byte)225, AccentColor.R, AccentColor.G, AccentColor.B));

        foreach (var port in InputPorts.Concat(OutputPorts))
        {
            port.UpdateBrushes(AccentColor);
        }

        Graph?.RefreshConnectionAppearance(this);
        Graph?.RequestVisualRefresh();
    }
}
