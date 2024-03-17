using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace TreeGrid
{
    /// <summary>
    /// Interaction logic for TreeGrid.xaml
    /// </summary>
    public partial class TreeGridControl : UserControl
    {
        private List<string> ColumnNames { get; set; }

        #region dependency properties
        public static readonly DependencyProperty ColumnManagerProperty = DependencyProperty.Register(
            "ColumnManager",
            typeof(IColumnManager),
            typeof(TreeGridControl),
            new PropertyMetadata(null, OnColumnManagerChanged));

        public IColumnManager ColumnManager
        {
            get { return (IColumnManager)GetValue(ColumnManagerProperty); }
            set { SetValue(ColumnManagerProperty, value); }
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(TreeGridControl), new PropertyMetadata(null));

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            "Items",
            typeof(ObservableCollection<TreeItem>),
            typeof(TreeGridControl),
            new PropertyMetadata(new ObservableCollection<TreeItem>()));

        public ObservableCollection<TreeItem> Items
        {
            get { return (ObservableCollection<TreeItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        private static void OnColumnManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TreeGridControl)d;
            control.ColumnManagerChanged();
        }

        #endregion

        private void ColumnManagerChanged()
        {
            GenerateGridHeaderColumns();
            var firstColumn = ColumnManager.GetFirstColumn();
            firstColumn.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Width")
                {
                    foreach(var treeItem in this.Items)
                    {
                        treeItem.UpdateWidth(firstColumn.Width.Value);
                    }
                }
            };
        }

        private IEnumerable<string> GetColumnNames()
            => ColumnManager.GetType().GetProperties(
                BindingFlags.Public | BindingFlags.Instance).Where(p => p.PropertyType == typeof(ColumnData))
                .Select(p => p.Name);

        private void GenerateGridHeaderColumns()
        {
            if (ColumnManager == null)
            {
                return;
            }

            GridHeader.Columns.Clear();

            // Use reflection to get the properties of the ColumnManager
            ColumnNames = GetColumnNames().ToList();
            foreach (var columnName in ColumnNames)
            {
                GridHeader.Columns.Add(
                    CreateBoundGridHeaderColumn(columnName)
                );
            }
        }
        
        private DataGridTextColumn CreateBoundGridHeaderColumn(string columnName)
        {
            var column = new DataGridTextColumn();
            
            BindingOperations.SetBinding(column, DataGridTextColumn.HeaderProperty, new Binding($"{columnName}") { Source = ColumnManager });
            BindingOperations.SetBinding(column, DataGridTextColumn.DisplayIndexProperty, new Binding($"{columnName}.DisplayIndex") { Source = ColumnManager, FallbackValue = 1, Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(column, DataGridTextColumn.WidthProperty, new Binding($"{columnName}.Width") { Source = ColumnManager, Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(column, DataGridTextColumn.VisibilityProperty, new Binding($"{columnName}.IsVisible") { Source = ColumnManager, Mode = BindingMode.TwoWay, Converter = new BooleanToVisibilityConverter() });
            BindingOperations.SetBinding(column, DataGridTextColumn.SortDirectionProperty, new Binding($"{columnName}.SortDirection") { Source = ColumnManager, Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(column, DataGridTextColumn.MinWidthProperty, new Binding($"{columnName}.MinWidth") { Source = ColumnManager });

            if (column.DisplayIndex == 0)
            {
                column.CanUserReorder = false;
            }
            return column;
        }

        public void TreeItemContentControlLoaded(ContentPresenter contentPresenter)
        {
            var grid = contentPresenter.GetAscendant<Grid>();
            var panel = GetPanel(contentPresenter);
            SetTreeItemGridColumnDefinitions(grid);
            AddTreeItemElementsToGrid(panel, grid);
        }

        private void SetTreeItemGridColumnDefinitions(Grid grid)
        {
            for (var i = 1; i < ColumnManager.Columns.Length; i++)
            {
                Binding widthBinding = new Binding
                {
                    Source = ColumnManager,
                    Path = new PropertyPath($"Columns[{i}].GridWidth")
                };
                
                var columnDefinition = new ColumnDefinition();
                BindingOperations.SetBinding(columnDefinition, ColumnDefinition.WidthProperty, widthBinding);
                grid.ColumnDefinitions.Add(columnDefinition);

            }
        }

        private void AddTreeItemElementsToGrid(Panel panel, Grid grid)
        {
            grid.Children.Remove(this);

            var numChildren = VisualTreeHelper.GetChildrenCount(panel);
            for (int i = 0; i < numChildren; i++)
            {
                var child = Reparent(panel, grid);
                BindTreeItem(child, i);
            }
        }

        private void BindTreeItem(UIElement child, int index)
        {
            var displayIndexBinding = new Binding
            {
                Source = this,
                Path = new PropertyPath($"ColumnManager.{this.ColumnNames[index]}.DisplayIndex")
            };

            BindingOperations.SetBinding(child, Grid.ColumnProperty, displayIndexBinding);

            var visibilityBinding = new Binding
            {
                Source = this,
                Path = new PropertyPath($"ColumnManager.{this.ColumnNames[index]}.IsVisible"),
                Converter = new BooleanToVisibilityConverter()
            };

            BindingOperations.SetBinding(child, UIElement.VisibilityProperty, visibilityBinding);

        }

        private UIElement Reparent(Panel panel, Grid grid)
        {
            var child = VisualTreeHelper.GetChild(panel, 0) as UIElement;
            panel.Children.Remove(child);
            grid.Children.Add(child);
            return child;
        }

        private Panel GetPanel(ContentPresenter contentPresenter) => VisualTreeHelper.GetChild(contentPresenter, 0) as Panel;

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is TreeViewItem treeViewItem))
                return;
           
        }

        private void GridHeader_ColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            var firstColumn = ColumnManager.GetFirstColumn();
            if (firstColumn.DisplayIndex != 0)
            {
                for (var i = 0; i < originalDisplayIndices.Count; i++)
                {
                    ColumnManager.Columns[i].DisplayIndex = originalDisplayIndices[i];
                }
            } else
            {
                ColumnManager.SortColumnsArray();
            }
           
        }

        private void DataGridColumnHeader_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Down)
                return;
            e.Handled = true;
            this.TreeView.Focus();
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);
            if (!(this.TreeView.SelectedItem is TreeItem selectedItem))
                return;
            selectedItem.IsSelectionActive = (bool)e.NewValue;
        }

        private void TreeView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.GridHeaderScrollViewer.Width = e.ViewportWidth;
            if (e.HorizontalChange == 0.0)
                return;
            this.GridHeaderScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
        }

        private void TreeViewItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) => e.Handled = true;

        public TreeGridControl()
        {
            InitializeComponent();
            this.TreeView.AddHandler(ScrollViewer.ScrollChangedEvent, (Delegate)new ScrollChangedEventHandler(this.TreeView_ScrollChanged));
            this.GridHeader.ColumnReordered += new EventHandler<DataGridColumnEventArgs>(this.GridHeader_ColumnReordered);
            this.GridHeader.ColumnHeaderDragStarted += this.GridHeader_ColumnHeaderDragStarted;
        }


        private List<int> originalDisplayIndices;
        private void GridHeader_ColumnHeaderDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            originalDisplayIndices = ColumnManager.Columns.Select(c => c.DisplayIndex).ToList();
        }

        private void DataGridColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if(sender is DataGridColumnHeader gridColumnHeader)
            {
                Sort(gridColumnHeader.DisplayIndex);
            }
        }

        private void Sort(int columnIndex)
        {
            foreach(var column in ColumnManager.Columns)
            {
                if(column.DisplayIndex == columnIndex)
                {
                    if(column.SortDirectionValue == null)
                    {
                        column.SortDirectionValue = ListSortDirection.Ascending;
                    }
                    else if(column.SortDirectionValue == ListSortDirection.Ascending)
                    {
                        column.SortDirectionValue = ListSortDirection.Descending;
                    }
                    else
                    {
                        column.SortDirectionValue = ListSortDirection.Ascending;
                    }
                }
                else
                {
                    column.SortDirectionValue = null;
                }
                
                
            }
        }
    }
}
