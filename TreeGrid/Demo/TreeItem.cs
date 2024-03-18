using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TreeGrid
{
    public class TreeItem : TreeItemBase
    {
        private bool _isExpanded;
        private ObservableCollection<ITreeItem> observableChildren = new ObservableCollection<ITreeItem>();
        public TreeItem(string name,IEnumerable<TreeItem> children = null)
        {
            Name = name;
            if(children != null)
            {
                foreach (var child in children)
                {
                    observableChildren.Add(child);
                    child.Parent = this;
                }
            }
            Children = observableChildren;
        }
        private string _name;
        public string Name { 
            get => this._name;
            set => this.Set<string>(ref this._name, value, nameof(Name));
        }

        public override bool IsExpanded
        {
            get => this._isExpanded;
            set
            {
                this.Set<bool>(ref this._isExpanded, value, nameof(IsExpanded));
                //if (this.ChildrenLoaded || !value)
                //    return;
                //this.Children.Clear();
                //this.LoadChildren();
                this.AdjustWidth(this._rootWidth);
                //this.ChildrenLoaded = true;
            }
        }
    }
}
