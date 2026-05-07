using Avalonia;
using Avalonia.Media;
using ReactorDesigner.Models;
using System.Collections.ObjectModel;

namespace ReactorDesigner.ViewModels;

public sealed class ConnectionViewModel : ViewModelBase
{
    public ConnectionViewModel(ConnectionModel model, PortViewModel sourcePort, PortViewModel targetPort)
    {
        Model = model;
        SourcePort = sourcePort;
        TargetPort = targetPort;
        strokeBrush = Brushes.Transparent;
        handleFillBrush = new SolidColorBrush(Color.Parse("#10161E"));
        handleStrokeBrush = Brushes.Transparent;
        BendPoints = new ObservableCollection<ConnectionBendPointViewModel>(
            model.BendPoints.Select(point => new ConnectionBendPointViewModel(point, this)));
        RefreshAppearance();
    }

    public ConnectionModel Model { get; }

    public Guid Id => Model.Id;

    public PortViewModel SourcePort { get; }

    public PortViewModel TargetPort { get; }

    public IBrush StrokeBrush
    {
        get => strokeBrush;
        private set => SetProperty(ref strokeBrush, value);
    }

    public IBrush HandleFillBrush
    {
        get => handleFillBrush;
        private set => SetProperty(ref handleFillBrush, value);
    }

    public IBrush HandleStrokeBrush
    {
        get => handleStrokeBrush;
        private set => SetProperty(ref handleStrokeBrush, value);
    }

    public ObservableCollection<ConnectionBendPointViewModel> BendPoints { get; }

    public double Thickness => 3;

    public GraphViewModel? Graph { get; private set; }

    private IBrush strokeBrush;
    private IBrush handleFillBrush;
    private IBrush handleStrokeBrush;

    public void AttachToGraph(GraphViewModel graph)
    {
        Graph = graph;
    }

    public void RefreshAppearance()
    {
        var sourceColor = SourcePort.Node.AccentColor;
        StrokeBrush = new SolidColorBrush(Color.FromArgb(235, sourceColor.R, sourceColor.G, sourceColor.B));
        HandleStrokeBrush = new SolidColorBrush(Color.FromArgb(235, sourceColor.R, sourceColor.G, sourceColor.B));
        Graph?.RequestVisualRefresh();
    }

    public ConnectionBendPointViewModel InsertBendPoint(int index, Point position)
    {
        var model = new ConnectionBendPointModel
        {
            X = position.X,
            Y = position.Y
        };

        var bendPoint = new ConnectionBendPointViewModel(model, this);
        Model.BendPoints.Insert(index, model);
        BendPoints.Insert(index, bendPoint);
        NotifyRouteChanged();
        return bendPoint;
    }

    public void RemoveBendPoint(ConnectionBendPointViewModel bendPoint)
    {
        if (!BendPoints.Remove(bendPoint))
        {
            return;
        }

        Model.BendPoints.Remove(bendPoint.Model);
        NotifyRouteChanged();
    }

    public void NotifyRouteChanged()
    {
        Graph?.RequestVisualRefresh();
    }
}
