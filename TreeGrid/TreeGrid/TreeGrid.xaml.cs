using System;
using System.Collections.Generic;
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

        public ITreeGridViewModel ViewModel
        {
            get { return (ITreeGridViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ITreeGridViewModel), typeof(TreeGridControl), new PropertyMetadata(null,OnViewModelChanged));

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TreeGridControl)d;
            control.Initialize();
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(TreeGridControl), new PropertyMetadata(null));

        #endregion

        private void Initialize()
        {
            InitializeColumns();
            if(ViewModel != null)
            {
                new DragBeforeFirstBlocker(ViewModel.ColumnManager, GridHeader, () => ViewModel.ColumnManager.SortColumnsArray());
            }
        }
        private void InitializeColumns()
        {
            if(ViewModel == null)
            {
                return;
            }
            GenerateGridHeaderColumns();
            var firstColumn = ViewModel.ColumnManager.Columns[0];
            firstColumn.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Width")
                {
                    foreach(var treeItem in this.ViewModel.Items)
                    {
                        treeItem.UpdateWidth(firstColumn.Width.Value);
                    }
                }
            };
        }

        private IEnumerable<string> GetColumnNames()
            => ViewModel.ColumnManager.GetType().GetProperties(
                BindingFlags.Public | BindingFlags.Instance).Where(p => p.PropertyType == typeof(ColumnData))
                .Select(p => p.Name);

        private void GenerateGridHeaderColumns()
        {
            GridHeader.Columns.Clear();

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
            var ColumnManager = ViewModel.ColumnManager;
            // necessary to prevent duplicate column sort icons
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
            for (var i = 1; i < ViewModel.ColumnManager.Columns.Length; i++)
            {
                Binding widthBinding = new Binding
                {
                    Source = ViewModel.ColumnManager,
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
                Path = GetColumnManagerPropertyPath("DisplayIndex", index)
            };

            BindingOperations.SetBinding(child, Grid.ColumnProperty, displayIndexBinding);
            if(index != 0)
            {
                var visibilityBinding = new Binding
                {
                    Source = this,
                    Path = GetColumnManagerPropertyPath("IsVisible", index),
                    Converter = new BooleanToVisibilityConverter()
                };

                BindingOperations.SetBinding(child, UIElement.VisibilityProperty, visibilityBinding);
            }
        }

        private PropertyPath GetColumnManagerPropertyPath(string property, int index)
        {
            return new PropertyPath($"ViewModel.ColumnManager.{this.ColumnNames[index]}.{property}");
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
            if (!(this.TreeView.SelectedItem is ITreeItem selectedItem))
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
            this.TreeView.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(this.TreeView_SelectedItemChanged);
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(ViewModel != null)
            {
                ViewModel.SelectedTreeViewItem = (ITreeItem)e.NewValue;
            }
        }

        private void DataGridColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if(sender is DataGridColumnHeader gridColumnHeader)
            {
                ViewModel.Sort(gridColumnHeader.DisplayIndex);
            }
        }
    }
}
