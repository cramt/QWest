using Model.Geographic;
using QWest.DataAcess;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace QWest.Admin {
    /// <summary>
    /// Interaction logic for DisplayCountry.xaml
    /// </summary>
    public partial class DisplayCountry : Window {
        private List<Country> _countries;
        public DisplayCountry(IEnumerable<Country> countires) {
            InitializeComponent();
            _countries = countires.ToList();

            this.dataGrid1.ItemsSource = new ObservableCollection<Country>(_countries);
        }

        private void EditButtonClick(object sender, RoutedEventArgs e) {

        }

        private async void SubdivisionButtonClick(object sender, RoutedEventArgs e) {
            Country county = (sender as Button).DataContext as Country;
            var results = await DAO.Geography.GetSubdivisions(county);
            if (results == null) {
                MessageBox.Show("this location does not have any subdivisions");
                return;
            }
            new DisplaySubdivision(results).Show();
        }
    }
}
