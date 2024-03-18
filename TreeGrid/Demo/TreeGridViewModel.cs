using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TreeGrid
{
    public class TreeGridViewModel : ITreeGridViewModel
    {
        private ColumnManager _columnManager = new ColumnManager();
        public TreeGridViewModel()
        {
            _items[0].UpdateWidth(_columnManager.FirstColumn.Width.Value);
        }
        public IColumnManager ColumnManager => _columnManager;
        private ObservableCollection<TreeItem> _items = new ObservableCollection<TreeItem>
        {
           new TreeItem(
               "Root",
               new TreeItem[]{
                   new TreeItem(
                       "Child",
                       new TreeItem[]{new TreeItem("GC") })})
        };
        public IEnumerable<ITreeItem> Items => _items;

        private ITreeItem _selectedTreeViewItem;
        public ITreeItem SelectedTreeViewItem
        {
            get => _selectedTreeViewItem;
            set
            {
                _selectedTreeViewItem = value;
            }
        }

        public string TreeViewAutomationName { get; } = "DemoTreeView";

        public void Sort(int columnIndex)
        {
            _columnManager.SortColumns(columnIndex);
        }

        internal void Update()
        {
            this._items[0].Name = "Root" + DateTime.Now.Second;
        }
    }
}
