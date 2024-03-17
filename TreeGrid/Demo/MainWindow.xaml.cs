using System.Windows;

namespace TreeGrid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            this .DataContext = new MainViewModel();
            InitializeComponent();
        }
    }
}
