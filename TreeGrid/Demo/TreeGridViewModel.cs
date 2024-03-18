using System;
using System.Collections.ObjectModel;

namespace TreeGrid
{
    public class TreeGridViewModel : TreeGridViewModelBase<TreeItem,ColumnManager>
    {
        public TreeGridViewModel()
        {
            columnManagerImpl = new ColumnManager();
            _items[0].UpdateWidth(columnManagerImpl.FirstColumn.Width.Value);
            TreeViewAutomationName = "DemoTreeView";
            SetItems(_items);
        }
        private ObservableCollection<TreeItem> _items = new ObservableCollection<TreeItem>
        {
           new TreeItem(
               "Root",
               new TreeItem[]{
                   new TreeItem(
                       "Child",
                       new TreeItem[]{new TreeItem("GC") })})
        };

        public override void Sort(int columnIndex)
        {
            columnManagerImpl.SortColumns(columnIndex);
        }

        internal void Update()
        {
            this._items[0].Name = "Root" + DateTime.Now.Second;
        }

    }
}
