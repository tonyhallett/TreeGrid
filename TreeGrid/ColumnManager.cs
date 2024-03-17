using System.Linq;

namespace TreeGrid
{
    public class ColumnManager : ObservableBase, IColumnManager
    {
        public ColumnData[] Columns { get; set; }
        public ColumnData FirstColumn { get; } = new ColumnData("First", 0, true, 450);
        public ColumnData SecondColumn { get; } = new ColumnData("Second", 1, true, 150.0);
        public ColumnData ThirdColumn { get; } = new ColumnData("Third", 2, true, 150.0);
        public ColumnManager()
        {
            Columns = new ColumnData[] { FirstColumn, SecondColumn, ThirdColumn };
        }
        public void SortColumnsArray()
        {
            this.Columns = this.Columns.OrderBy(c => c.DisplayIndex).ToArray();
            this.OnPropertyChanged("Columns");
        }

        public ColumnData GetFirstColumn() => FirstColumn;
    }
}
