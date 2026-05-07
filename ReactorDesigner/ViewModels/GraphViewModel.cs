using System.Collections.ObjectModel;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactorDesigner.Controls;
using ReactorDesigner.Models;

namespace ReactorDesigner.ViewModels;

public partial class GraphViewModel : ViewModelBase
{
    private const string CustomNodeIconPath = "M4 4h16v16H4zm3 3v10h10V7zm2 2h6v6H9z";
    private const double CanvasMenuWidth = 232;
    private const double CanvasMenuHeight = 324;
    private const double NodeEditorWidth = 372;
    private const double NodeEditorHeight = 560;
    private const double ConnectionMenuWidth = 228;
    private const double ConnectionMenuHeight = 132;

    private readonly Dictionary<NodeKind, int> _kindCounters = new();
    private readonly Dictionary<NodeKind, NodeTemplate> _templates;
    private ConnectionBendPointViewModel? _activeBendPoint;
    private ConnectionBendPointViewModel? _contextBendPoint;
    private ConnectionRouteBuilder.ConnectionSegmentHit? _contextSegmentHit;
    private Point _canvasMenuSpawnPosition;
    private Point _connectionMenuGraphPosition;
    private int _spawnCounter;

    public GraphViewModel()
    {
        var templates = ProcessNodeCatalog.GetTemplates();
        _templates = templates.ToDictionary(template => template.Kind);

        Nodes = new ObservableCollection<NodeViewModel>();
        Connections = new ObservableCollection<ConnectionViewModel>();
        ToolboxGroups = new ObservableCollection<ToolboxGroupViewModel>(
            templates
                .GroupBy(template => template.Group)
                .Select(group => new ToolboxGroupViewModel(
                    group.Key,
                    group.Key == "Process Equipment"
                        ? "Drag major plant assets onto the canvas."
                        : group.Key == "Instrumentation"
                            ? "Drop measurement and signal nodes into the graph."
                            : "Use control nodes for routing and regulation.",
                    group.Select(template => new ToolboxItemViewModel(template)))));

        PortCountOptions = new ObservableCollection<int>(Enumerable.Range(0, 17));
    }

    public ObservableCollection<NodeViewModel> Nodes { get; }

    public ObservableCollection<ConnectionViewModel> Connections { get; }

    public ObservableCollection<ToolboxGroupViewModel> ToolboxGroups { get; }

    public ObservableCollection<int> PortCountOptions { get; }

    public double WorkspaceWidth => 5200;

    public double WorkspaceHeight => 3600;

    public bool IsConnectionPreviewActive => PreviewSourcePort is not null;

    public bool IsBendPointDragActive => _activeBendPoint is not null;

    public bool HasSelectedNode => SelectedNode is not null;

    public bool HasNoSelectedNode => SelectedNode is null;

    public bool ConnectionMenuCanAddBendPoint => _contextSegmentHit is not null;

    public bool ConnectionMenuCanRemoveBendPoint => _contextBendPoint is not null;

    [ObservableProperty]
    private PortViewModel? previewSourcePort;

    [ObservableProperty]
    private Point previewTargetPoint;

    [ObservableProperty]
    private NodeViewModel? selectedNode;

    [ObservableProperty]
    private string customNodeTitle = "Custom Unit";

    [ObservableProperty]
    private string customNodeAccentHex = "#3B82F6";

    [ObservableProperty]
    private int customNodeInputCount = 1;

    [ObservableProperty]
    private int customNodeOutputCount = 1;

    [ObservableProperty]
    private bool isCanvasMenuOpen;

    [ObservableProperty]
    private Point canvasMenuPosition;

    [ObservableProperty]
    private bool isNodeEditorOpen;

    [ObservableProperty]
    private Point nodeEditorPosition;

    [ObservableProperty]
    private bool isConnectionMenuOpen;

    [ObservableProperty]
    private Point connectionMenuPosition;

    public event EventHandler? VisualRefreshRequested;

    public NodeViewModel CreateNode(NodeKind kind, Point dropPosition)
    {
        var template = _templates[kind];
        var node = CreateNodeFromTemplate(template, dropPosition, null);
        Nodes.Add(node);
        BringNodeToFront(node);
        SelectNode(node);
        RequestVisualRefresh();
        return node;
    }

    public NodeViewModel CreateNodeFromToolbox(NodeKind kind)
    {
        return CreateNode(kind, GetNextSpawnPoint());
    }

    public void BringNodeToFront(NodeViewModel node)
    {
        var currentIndex = Nodes.IndexOf(node);
        if (currentIndex < 0 || currentIndex == Nodes.Count - 1)
        {
            return;
        }

        Nodes.RemoveAt(currentIndex);
        Nodes.Add(node);
    }

    public void SelectNode(NodeViewModel? node)
    {
        SelectedNode = node;
    }

    public void ShowCanvasAddMenu(Point position)
    {
        CloseTransientPopups();
        SelectNode(null);
        _canvasMenuSpawnPosition = position;
        CanvasMenuPosition = ClampPopupPosition(position, CanvasMenuWidth, CanvasMenuHeight);
        IsCanvasMenuOpen = true;
    }

    public void ShowNodeEditor(NodeViewModel node, Point position)
    {
        CloseTransientPopups();
        SelectNode(node);
        NodeEditorPosition = ClampPopupPosition(
            new Point(position.X + 14, position.Y + 14),
            NodeEditorWidth,
            NodeEditorHeight);
        IsNodeEditorOpen = true;
    }

    public bool TryShowConnectionMenu(Point position)
    {
        var bendPoint = ConnectionRouteBuilder.FindNearestBendPoint(Connections, position);
        var segmentHit = bendPoint is null
            ? ConnectionRouteBuilder.FindNearestSegment(Connections, position)
            : null;

        if (bendPoint is null && segmentHit is null)
        {
            return false;
        }

        CloseTransientPopups();
        _connectionMenuGraphPosition = position;
        _contextBendPoint = bendPoint;
        _contextSegmentHit = segmentHit;
        ConnectionMenuPosition = ClampPopupPosition(position, ConnectionMenuWidth, ConnectionMenuHeight);
        IsConnectionMenuOpen = true;
        OnPropertyChanged(nameof(ConnectionMenuCanAddBendPoint));
        OnPropertyChanged(nameof(ConnectionMenuCanRemoveBendPoint));
        return true;
    }

    public void CloseTransientPopups()
    {
        IsCanvasMenuOpen = false;
        IsNodeEditorOpen = false;
        IsConnectionMenuOpen = false;
        _contextBendPoint = null;
        _contextSegmentHit = null;
        OnPropertyChanged(nameof(ConnectionMenuCanAddBendPoint));
        OnPropertyChanged(nameof(ConnectionMenuCanRemoveBendPoint));
    }

    public void BeginConnectionPreview(PortViewModel sourcePort, Point currentPosition)
    {
        if (sourcePort.Direction != PortDirection.Output)
        {
            return;
        }

        PreviewSourcePort = sourcePort;
        PreviewTargetPoint = currentPosition;
        RequestVisualRefresh();
    }

    public void UpdateConnectionPreview(Point currentPosition)
    {
        if (!IsConnectionPreviewActive)
        {
            return;
        }

        PreviewTargetPoint = currentPosition;
        RequestVisualRefresh();
    }

    public void TryCompleteConnection(PortViewModel? targetPort)
    {
        if (PreviewSourcePort is null || targetPort is null)
        {
            CancelConnectionPreview();
            return;
        }

        if (!CanConnect(PreviewSourcePort, targetPort))
        {
            CancelConnectionPreview();
            return;
        }

        var alreadyExists = Connections.Any(connection =>
            connection.SourcePort.Id == PreviewSourcePort.Id &&
            connection.TargetPort.Id == targetPort.Id);

        if (!alreadyExists)
        {
            var model = new ConnectionModel
            {
                SourcePortId = PreviewSourcePort.Id,
                TargetPortId = targetPort.Id
            };

            AddConnection(new ConnectionViewModel(model, PreviewSourcePort, targetPort));
        }

        CancelConnectionPreview();
    }

    public void CancelConnectionPreview()
    {
        PreviewSourcePort = null;
        RequestVisualRefresh();
    }

    public bool TryBeginBendPointDrag(Point position)
    {
        _activeBendPoint = ConnectionRouteBuilder.FindNearestBendPoint(Connections, position);
        return _activeBendPoint is not null;
    }

    public void UpdateBendPointDrag(Point position)
    {
        if (_activeBendPoint is null)
        {
            return;
        }

        _activeBendPoint.Position = position;
    }

    public void EndBendPointDrag()
    {
        _activeBendPoint = null;
    }

    public bool TryInsertBendPoint(Point position)
    {
        var hit = ConnectionRouteBuilder.FindNearestSegment(Connections, position);
        if (hit is null)
        {
            return false;
        }

        _activeBendPoint = hit.Value.Connection.InsertBendPoint(hit.Value.InsertIndex, hit.Value.InsertPosition);
        RequestVisualRefresh();
        return true;
    }

    public bool TryRemoveBendPoint(Point position)
    {
        var bendPoint = ConnectionRouteBuilder.FindNearestBendPoint(Connections, position);
        if (bendPoint is null)
        {
            return false;
        }

        if (ReferenceEquals(_activeBendPoint, bendPoint))
        {
            _activeBendPoint = null;
        }

        bendPoint.Connection.RemoveBendPoint(bendPoint);
        RequestVisualRefresh();
        return true;
    }

    public void HandlePortTopologyChanged(NodeViewModel node, IReadOnlyCollection<Guid> removedPortIds)
    {
        if (removedPortIds.Count > 0)
        {
            var portIdSet = removedPortIds.ToHashSet();
            var connectionsToRemove = Connections
                .Where(connection => portIdSet.Contains(connection.SourcePort.Id) || portIdSet.Contains(connection.TargetPort.Id))
                .ToList();

            foreach (var connection in connectionsToRemove)
            {
                Connections.Remove(connection);
            }
        }

        node.RefreshPortNames();
        RequestVisualRefresh();
    }

    public void RefreshConnectionAppearance(NodeViewModel node)
    {
        foreach (var connection in Connections.Where(connection => connection.SourcePort.Node.Id == node.Id))
        {
            connection.RefreshAppearance();
        }

        RequestVisualRefresh();
    }

    public void RequestVisualRefresh()
    {
        VisualRefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void CreateCustomNode()
    {
        var node = CreateCustomNodeModel(GetNextSpawnPoint());
        Nodes.Add(node);
        BringNodeToFront(node);
        SelectNode(node);
        RequestVisualRefresh();
    }

    [RelayCommand]
    private void CreateNodeFromCanvasMenu(string? rawKind)
    {
        if (!Enum.TryParse<NodeKind>(rawKind, out var kind))
        {
            return;
        }

        CreateNode(kind, _canvasMenuSpawnPosition);
        CloseTransientPopups();
    }

    [RelayCommand]
    private void CreateCustomNodeFromCanvasMenu()
    {
        var node = CreateCustomNodeModel(_canvasMenuSpawnPosition);
        Nodes.Add(node);
        BringNodeToFront(node);
        SelectNode(node);
        CloseTransientPopups();
        RequestVisualRefresh();
    }

    [RelayCommand]
    private void DuplicateSelectedNode()
    {
        if (SelectedNode is null)
        {
            return;
        }

        var duplicatePosition = new Point(SelectedNode.X + 48, SelectedNode.Y + 48);
        var duplicate = CreateNodeFromModel(
            CloneNodeModel(SelectedNode.Model, $"{SelectedNode.Title} Copy"),
            duplicatePosition);
        Nodes.Add(duplicate);
        BringNodeToFront(duplicate);
        SelectNode(duplicate);
        RequestVisualRefresh();
    }

    [RelayCommand]
    private void DeleteSelectedNode()
    {
        if (SelectedNode is null)
        {
            return;
        }

        var node = SelectedNode;
        var portIds = node.InputPorts.Select(port => port.Id)
            .Concat(node.OutputPorts.Select(port => port.Id))
            .ToHashSet();

        var connectionsToRemove = Connections
            .Where(connection => portIds.Contains(connection.SourcePort.Id) || portIds.Contains(connection.TargetPort.Id))
            .ToList();

        foreach (var connection in connectionsToRemove)
        {
            Connections.Remove(connection);
        }

        Nodes.Remove(node);
        SelectNode(null);
        CloseTransientPopups();
        RequestVisualRefresh();
    }

    [RelayCommand]
    private void AddSelectedInputPort()
    {
        if (SelectedNode is null)
        {
            return;
        }

        SelectedNode.InputCount += 1;
    }

    [RelayCommand]
    private void RemoveSelectedInputPort()
    {
        if (SelectedNode is null || SelectedNode.InputCount == 0)
        {
            return;
        }

        SelectedNode.InputCount -= 1;
    }

    [RelayCommand]
    private void AddSelectedOutputPort()
    {
        if (SelectedNode is null)
        {
            return;
        }

        SelectedNode.OutputCount += 1;
    }

    [RelayCommand]
    private void RemoveSelectedOutputPort()
    {
        if (SelectedNode is null || SelectedNode.OutputCount == 0)
        {
            return;
        }

        SelectedNode.OutputCount -= 1;
    }

    [RelayCommand]
    private void IncreaseCustomInputCount()
    {
        CustomNodeInputCount = Math.Clamp(CustomNodeInputCount + 1, 0, 16);
    }

    [RelayCommand]
    private void DecreaseCustomInputCount()
    {
        CustomNodeInputCount = Math.Clamp(CustomNodeInputCount - 1, 0, 16);
    }

    [RelayCommand]
    private void IncreaseCustomOutputCount()
    {
        CustomNodeOutputCount = Math.Clamp(CustomNodeOutputCount + 1, 0, 16);
    }

    [RelayCommand]
    private void DecreaseCustomOutputCount()
    {
        CustomNodeOutputCount = Math.Clamp(CustomNodeOutputCount - 1, 0, 16);
    }

    [RelayCommand]
    private void AddBendPointFromConnectionMenu()
    {
        if (_contextSegmentHit is null)
        {
            return;
        }

        _contextSegmentHit.Value.Connection.InsertBendPoint(
            _contextSegmentHit.Value.InsertIndex,
            _contextSegmentHit.Value.InsertPosition);
        CloseTransientPopups();
        RequestVisualRefresh();
    }

    [RelayCommand]
    private void RemoveBendPointFromConnectionMenu()
    {
        if (_contextBendPoint is null)
        {
            return;
        }

        _contextBendPoint.Connection.RemoveBendPoint(_contextBendPoint);
        CloseTransientPopups();
        RequestVisualRefresh();
    }

    [RelayCommand]
    private void ClosePopups()
    {
        CloseTransientPopups();
    }

    private bool CanConnect(PortViewModel sourcePort, PortViewModel targetPort)
    {
        return sourcePort.Direction == PortDirection.Output &&
               targetPort.Direction == PortDirection.Input &&
               sourcePort.Node.Id != targetPort.Node.Id;
    }

    private NodeViewModel CreateNodeFromTemplate(NodeTemplate template, Point dropPosition, string? explicitTitle)
    {
        var model = new NodeModel
        {
            Kind = template.Kind,
            Title = explicitTitle ?? GetNextTitle(template.Title, template.Kind),
            Description = template.Description,
            AccentHex = template.AccentHex,
            IconPathData = template.IconPathData
        };

        foreach (var inputPort in template.InputPorts)
        {
            model.InputPorts.Add(new PortModel
            {
                Label = inputPort.Label,
                Direction = inputPort.Direction,
                ConnectionType = inputPort.ConnectionType
            });
        }

        foreach (var outputPort in template.OutputPorts)
        {
            model.OutputPorts.Add(new PortModel
            {
                Label = outputPort.Label,
                Direction = outputPort.Direction,
                ConnectionType = outputPort.ConnectionType
            });
        }

        return CreateNodeFromModel(model, dropPosition);
    }

    private NodeViewModel CreateCustomNodeModel(Point dropPosition)
    {
        var model = new NodeModel
        {
            Kind = NodeKind.Custom,
            Title = string.IsNullOrWhiteSpace(CustomNodeTitle) ? "Custom Unit" : CustomNodeTitle.Trim(),
            Description = "User defined node",
            AccentHex = string.IsNullOrWhiteSpace(CustomNodeAccentHex) ? "#3B82F6" : CustomNodeAccentHex.Trim(),
            IconPathData = CustomNodeIconPath
        };

        for (var index = 0; index < CustomNodeInputCount; index++)
        {
            model.InputPorts.Add(new PortModel
            {
                Label = $"In {index + 1}",
                Direction = PortDirection.Input,
                ConnectionType = "ProcessStream"
            });
        }

        for (var index = 0; index < CustomNodeOutputCount; index++)
        {
            model.OutputPorts.Add(new PortModel
            {
                Label = $"Out {index + 1}",
                Direction = PortDirection.Output,
                ConnectionType = "ProcessStream"
            });
        }

        return CreateNodeFromModel(model, dropPosition);
    }

    private NodeViewModel CreateNodeFromModel(NodeModel model, Point dropPosition)
    {
        var node = new NodeViewModel(model);
        node.AttachToGraph(this);
        node.SetPosition(dropPosition.X, dropPosition.Y);
        return node;
    }

    private string GetNextTitle(string baseTitle, NodeKind kind)
    {
        if (!_kindCounters.TryGetValue(kind, out var count))
        {
            count = 0;
        }

        count += 1;
        _kindCounters[kind] = count;
        return $"{baseTitle} {count:00}";
    }

    private Point GetNextSpawnPoint()
    {
        var column = _spawnCounter % 4;
        var row = _spawnCounter / 4;
        _spawnCounter += 1;
        return new Point(180 + (column * 320), 180 + (row * 220));
    }

    private Point ClampPopupPosition(Point requestedPosition, double popupWidth, double popupHeight)
    {
        var maxX = Math.Max(12, WorkspaceWidth - popupWidth - 12);
        var maxY = Math.Max(12, WorkspaceHeight - popupHeight - 12);

        return new Point(
            Math.Clamp(requestedPosition.X, 12, maxX),
            Math.Clamp(requestedPosition.Y, 12, maxY));
    }

    private void AddConnection(ConnectionViewModel connection)
    {
        connection.AttachToGraph(this);
        connection.RefreshAppearance();
        Connections.Add(connection);
    }

    private static NodeModel CloneNodeModel(NodeModel source, string duplicatedTitle)
    {
        var clone = new NodeModel
        {
            Kind = source.Kind,
            Title = duplicatedTitle,
            Description = source.Description,
            AccentHex = source.AccentHex,
            IconPathData = source.IconPathData
        };

        foreach (var inputPort in source.InputPorts)
        {
            clone.InputPorts.Add(new PortModel
            {
                Label = inputPort.Label,
                Direction = inputPort.Direction,
                ConnectionType = inputPort.ConnectionType
            });
        }

        foreach (var outputPort in source.OutputPorts)
        {
            clone.OutputPorts.Add(new PortModel
            {
                Label = outputPort.Label,
                Direction = outputPort.Direction,
                ConnectionType = outputPort.ConnectionType
            });
        }

        return clone;
    }

    partial void OnPreviewSourcePortChanged(PortViewModel? value)
    {
        RequestVisualRefresh();
    }

    partial void OnPreviewTargetPointChanged(Point value)
    {
        RequestVisualRefresh();
    }

    partial void OnSelectedNodeChanged(NodeViewModel? value)
    {
        foreach (var node in Nodes)
        {
            node.IsSelected = ReferenceEquals(node, value);
        }

        OnPropertyChanged(nameof(HasSelectedNode));
        OnPropertyChanged(nameof(HasNoSelectedNode));
    }
}
