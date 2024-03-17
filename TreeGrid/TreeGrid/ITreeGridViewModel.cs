using System.Collections.Generic;

namespace TreeGrid
{
    public interface ITreeGridViewModel
    {
        IColumnManager ColumnManager { get; }
        IEnumerable<ITreeItem> Items { get; }
        ITreeItem SelectedTreeViewItem { get; set; }

        void Sort(int displayIndex);
    }
}
