using System.Collections.ObjectModel;
using ReactorDesigner.Models;

namespace ReactorDesigner.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<NodeModel> Nodes { get; set; } = new();

    public MainWindowViewModel()
    {
        Nodes.Add(new NodeModel
        {
            X = 100,
            Y = 100,
            Title = "Reaktor",
            ImagePath = "Assets/reactor.png"
        });

        Nodes.Add(new NodeModel
        {
            X = 450,
            Y = 300,
            Title = "Pumpe",
            ImagePath = "Assets/pump.png"
        });
    }
}