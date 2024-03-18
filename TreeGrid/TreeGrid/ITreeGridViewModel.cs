using System.Collections.Generic;

namespace TreeGrid
{
    public interface ITreeGridViewModel
    {
        IColumnManager ColumnManager { get; }
        IEnumerable<ITreeItem> Items { get; }
        ITreeItem SelectedTreeViewItem { get; set; }
        string TreeViewAutomationName { get; }
        void Sort(int displayIndex);
    }
}
