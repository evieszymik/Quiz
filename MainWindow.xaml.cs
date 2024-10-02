using Quiz.ViewModel;
using System.Windows;

namespace Quiz
{
    public partial class MainWindow : Window
    {
        public bool isRunning = false;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            
        }
        private void Window_Solve_Closed(object sender, System.EventArgs e)
        {
            isRunning = false;

        }
    }
    
}