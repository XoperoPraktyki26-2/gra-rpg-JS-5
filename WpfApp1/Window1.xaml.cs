using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfRpg;

namespace WpfApp1
{
    /// <summary>
    /// Logika interakcji dla klasy Window1.xaml
    /// </summary>
    public partial class Window1 : Window 
    {
        public Window1()
        {
            InitializeComponent();
        }
        private void InitGame()
        {
            newGameBtn.Click += new RoutedEventHandler(newGame);


        }
        private void newGame(object sender, RoutedEventArgs args)
        {
            MainWindow gameWindow = new MainWindow();
            gameWindow.ShowDialog();


            this.Close();
        }
    }
}

