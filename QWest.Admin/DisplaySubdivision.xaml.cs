using Model.Geographic;
using QWest.DataAccess;
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
    /// Interaction logic for DisplaySubdivision.xaml
    /// </summary>
    public partial class DisplaySubdivision : Window {
        private List<Subdivision> _subdivisions;
        public DisplaySubdivision(IEnumerable<Subdivision> subdivisions) {
            InitializeComponent();
            _subdivisions = subdivisions.ToList();

            this.dataGrid1.ItemsSource = new ObservableCollection<Subdivision>(_subdivisions);
        }

        private void EditButtonClick(object sender, RoutedEventArgs e) {
            Subdivision subdivision = (sender as Button).DataContext as Subdivision;
            new EditSubdivision(subdivision).Show();
        }

        private async void SubdivisionButtonClick(object sender, RoutedEventArgs e) {
            Subdivision subdivision = (sender as Button).DataContext as Subdivision;
            var results = await DAO.Geography.GetSubdivisions(subdivision);
            if (results == null) {
                MessageBox.Show("this location does not have any subdivisions");
                return;
            }
            new DisplaySubdivision(results).Show();
        }
    }
}
