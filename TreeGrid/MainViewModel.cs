using System.Collections.ObjectModel;

namespace TreeGrid
{
    public class MainViewModel
    {
        public ObservableCollection<TreeItem> Items { get; } = new ObservableCollection<TreeItem>
        {
           new TreeItem(
               "Root", 
               new TreeItem[]{ 
                   new TreeItem(
                       "Child", 
                       new TreeItem[]{new TreeItem("GC") })})
        };
        public ColumnManager ColumnManager { get; } = new ColumnManager();
        public MainViewModel()
        {
            Items[0].UpdateWidth(ColumnManager.FirstColumn.Width.Value);
        }
    }
}
