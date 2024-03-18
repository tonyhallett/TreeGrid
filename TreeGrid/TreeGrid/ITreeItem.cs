using System.Collections.Generic;

namespace TreeGrid
{
    public interface ITreeItem
    {
        bool IsSelectionActive { get; set; }

        void UpdateWidth(double value);
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
        ITreeItem Parent { get; set; }
        IEnumerable<ITreeItem> Children { get; }
    }
}
