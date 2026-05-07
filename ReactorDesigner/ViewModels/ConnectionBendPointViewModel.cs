using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactorDesigner.Models;

namespace ReactorDesigner.ViewModels;

public partial class ConnectionBendPointViewModel : ViewModelBase
{
    public ConnectionBendPointViewModel(ConnectionBendPointModel model, ConnectionViewModel connection)
    {
        Model = model;
        Connection = connection;
        x = model.X;
        y = model.Y;
    }

    public ConnectionBendPointModel Model { get; }

    public Guid Id => Model.Id;

    public ConnectionViewModel Connection { get; }

    [ObservableProperty]
    private double x;

    [ObservableProperty]
    private double y;

    public Point Position
    {
        get => new(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    partial void OnXChanged(double value)
    {
        Model.X = value;
        Connection.NotifyRouteChanged();
    }

    partial void OnYChanged(double value)
    {
        Model.Y = value;
        Connection.NotifyRouteChanged();
    }
}
