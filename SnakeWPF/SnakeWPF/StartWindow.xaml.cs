using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SnakeWPF
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private void Start_Btn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow=new MainWindow();
            mainWindow.Show();
            this.Hide();
        }

        private void Menu_Btn_Click(object sender, RoutedEventArgs e)
        {
          SettingsWindow settingsWindow=new SettingsWindow();
            settingsWindow.Show();
            this  .Hide();
        }

        private void Exit_Btn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
