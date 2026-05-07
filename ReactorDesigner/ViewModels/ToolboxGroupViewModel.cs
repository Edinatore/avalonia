using System.Collections.ObjectModel;

namespace ReactorDesigner.ViewModels;

public sealed class ToolboxGroupViewModel : ViewModelBase
{
    public ToolboxGroupViewModel(string title, string description, IEnumerable<ToolboxItemViewModel> items)
    {
        Title = title;
        Description = description;
        Items = new ObservableCollection<ToolboxItemViewModel>(items);
    }

    public string Title { get; }

    public string Description { get; }

    public ObservableCollection<ToolboxItemViewModel> Items { get; }
}
