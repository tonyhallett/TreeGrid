using System.Windows;

namespace TreeGrid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel mainViewModel;

        public MainWindow()
        {
            this.mainViewModel = new MainViewModel();
            this .DataContext = this.mainViewModel;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.mainViewModel.Update();
        }
    }
}
