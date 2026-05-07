namespace ReactorDesigner.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    public GraphViewModel Graph { get; } = new();
}
