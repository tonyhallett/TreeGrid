namespace TreeGrid
{
    public interface IColumnManager
    {
        ColumnData[] Columns { get; }

        ColumnData GetFirstColumn();

        void SortColumnsArray();
    }
}
