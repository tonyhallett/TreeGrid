﻿namespace TreeGrid
{
    public interface ITreeItem
    {
        bool IsSelectionActive { get; set; }

        void UpdateWidth(double value);
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
    }
}
