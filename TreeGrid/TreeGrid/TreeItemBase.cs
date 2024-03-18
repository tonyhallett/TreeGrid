﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace TreeGrid
{
    public abstract class TreeItemBase : ObservableBase, ITreeItem
    {
        private bool _isSelected;
        protected double _rootWidth;
        private GridLength _adjustedWidth;
        private bool _isSelectionActive = true;

        public bool IsSelected
        {
            get => this._isSelected;
            set
            {
                this.Set<bool>(ref this._isSelected, value, nameof(IsSelected));
                this._isSelectionActive = true;
                this.OnPropertyChanged("Background");
                this.OnPropertyChanged("Foreground");
            }
        }

        public Brush Background
        {
            get
            {
                if (!this.IsSelected)
                    return NotSelectedBackgroundBrush;
                return this.IsSelectionActive ? SelectedActiveBackgroundBrush : SelectedInactiveBackgroundBrush;
            }
        }
        private static Brush transparentBrush = new SolidColorBrush(Colors.Transparent);
        protected virtual Brush NotSelectedBackgroundBrush { get => transparentBrush; }
        protected virtual Brush SelectedInactiveBackgroundBrush { get; } = SystemColors.InactiveSelectionHighlightBrush;
        protected virtual Brush SelectedActiveBackgroundBrush { get; } = SystemColors.HighlightBrush;

        protected virtual Brush NotSelectedForegroundBrush { get; } = SystemColors.ControlTextBrush;
        protected virtual Brush SelectedInactiveForegroundBrush { get; } = SystemColors.InactiveSelectionHighlightTextBrush;
        protected virtual Brush SelectedActiveForegroundBrush { get; } = SystemColors.HighlightTextBrush;


        public Brush Foreground
        {
            get
            {
                if (!this.IsSelected)
                    return NotSelectedForegroundBrush;
                return this.IsSelectionActive ? SelectedActiveForegroundBrush : SelectedInactiveForegroundBrush;
            }
        }

        public bool IsSelectionActive
        {
            get => this._isSelectionActive;
            set
            {
                this._isSelectionActive = value;
                if (!this.IsSelected)
                    return;
                this.OnPropertyChanged("Background");
                this.OnPropertyChanged("Foreground");
            }
        }
        public abstract bool IsExpanded { get; set; }
        public ITreeItem Parent { get; set; }
        public IEnumerable<ITreeItem> Children { get; protected set; }
        
        public GridLength AdjustedWidth
        {
            get => this._adjustedWidth;
            set
            {
                this.Set<GridLength>(ref this._adjustedWidth, value, nameof(AdjustedWidth));
            }
        }
    
        private int WidthMultiplier => this.Parent != null ? (this.Parent as TreeItemBase).WidthMultiplier + 1 : 1;

        public void AdjustWidth(double width)
        {
            this._rootWidth = width;
            this.AdjustedWidth = new GridLength(this._rootWidth - (19 * this.WidthMultiplier));
            foreach (var treeItem in this.Children)
                treeItem.AdjustWidth(width);
        }
    }
}
