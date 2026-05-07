using CommunityToolkit.Mvvm.ComponentModel;

namespace ReactorDesigner.Models;

public partial class NodeModel : ObservableObject
{
    [ObservableProperty]
    private double x;

    [ObservableProperty]
    private double y;

    [ObservableProperty]
    private string title = "Node";

    [ObservableProperty]
    private string imagePath = "Assets/reactor.png";
}