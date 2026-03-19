using System.Windows;

namespace WpfRpg
{
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void newGame(object sender, RoutedEventArgs e)
        {
            var gameWindow = new MainWindow();
            gameWindow.Show();
            this.Close();
        }
    }
}