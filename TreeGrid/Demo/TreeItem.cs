using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace TreeGrid
{
    public class TreeItem : ObservableBase, ITreeItem
    {
        private bool _isSelected;
        private bool _isExpanded;
        private double _rootWidth;
        public TreeItem(string name,IEnumerable<TreeItem> children = null)
        {
            Name = name;
            if(children != null)
            {
                foreach (var child in children)
                {
                    Children.Add(child);
                    child.Parent = this;
                }
            }
        }
        private string _name;
        public string Name { 
            get => this._name;
            set => this.Set<string>(ref this._name, value, nameof(Name));
        }
        public ObservableCollection<TreeItem> Children { get;  } = new ObservableCollection<TreeItem>();

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
        private bool _isSelectionActive = true;
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

        public bool IsExpanded
        {
            get => this._isExpanded;
            set
            {
                this.Set<bool>(ref this._isExpanded, value, nameof(IsExpanded));
                //if (this.ChildrenLoaded || !value)
                //    return;
                //this.Children.Clear();
                //this.LoadChildren();
                this.UpdateWidth(this._rootWidth);
                //this.ChildrenLoaded = true;
            }
        }


        public Brush Background
        {
            get
            {
                if (!this.IsSelected)
                    return new SolidColorBrush(Colors.Transparent);
                // needs to be hooked to theme
                return !this.IsSelectionActive ? SelectedItemInactiveBackColor : SelectedItemActiveBackColor;
            }
        }

        private Brush SelectedItemInactiveBackColor = new SolidColorBrush(Colors.LightGray);
        private Brush SelectedItemInactiveForeColor = new SolidColorBrush(Colors.Black);

        private Brush SelectedItemActiveBackColor = new SolidColorBrush(Colors.LightBlue);
        private Brush SelectedItemActiveForeColor = new SolidColorBrush(Colors.White);
        
        private Brush NotSelectedForeground = new SolidColorBrush(Colors.Black);
        public Brush Foreground
        {
            get
            {
                if (!this.IsSelected)
                    return new SolidColorBrush(Colors.Pink);
                return !this.IsSelectionActive ? SelectedItemInactiveForeColor : SelectedItemActiveForeColor;
            }
        }

        public ITreeItem Parent { get; set; }
        public int WidthMultiplier => this.Parent != null ? (this.Parent as TreeItem).WidthMultiplier + 1 : 2;

        // just for now
        private GridLength _width;
        public GridLength Width
        {
            get => this._width;
            set
            {
                this.Set<GridLength>(ref this._width, value, nameof(Width));
            }
        }

        public void UpdateWidth(double width)
        {
            this._rootWidth = width;
            var widthMultiplierMultiplier = 19;//19
            var takeaway = -15; // 10
            this.Width = new GridLength(this._rootWidth - (double)(this.WidthMultiplier * widthMultiplierMultiplier) - takeaway);
            foreach (var treeItem in this.Children)
                treeItem.UpdateWidth(width);
        }

    }
}
