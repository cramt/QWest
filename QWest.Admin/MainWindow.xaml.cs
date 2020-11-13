using QWest.DataAcess;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QWest.Admin {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e) {
            //HACK, i dont know why this makes it work
            var countries = await Task.Factory.StartNew(() => DAO.Geography.GetCountries());
            new DisplayCountries(await countries).Show();
        }
    }
}
