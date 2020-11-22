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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class EditSubdivision : Window {
        public ObservableCollection<string> Names { get; set; }
        public ObservableCollection<GeopoliticalLocation> DisplayedLocations { get; private set; }
        private List<GeopoliticalLocation> AllLocations;
        public EditSubdivision(Subdivision subdivision) {
            InitializeComponent();
            DataContext = subdivision;

            if (subdivision.Names != null) {
                Names = new ObservableCollection<string>(subdivision.Names);
            }
            else {
                Names = new ObservableCollection<string>();
            }
            new Action( async () => await PopulateAllLocationsListBox())();
        }

        private async Task PopulateAllLocationsListBox() {
            ParentLocationListBox.Items.Add("Fetching locations...");
            AllLocations = GeopoliticalLocation.Traverse(await DAO.Geography.FetchEverythingParsed()).ToList();
            DisplayedLocations = new ObservableCollection<GeopoliticalLocation>(AllLocations);
            ParentLocationListBox.Items.Clear();
            Binding parentLocations = new Binding();
            parentLocations.Source = DisplayedLocations;
            ParentLocationListBox.SetBinding(ItemsControl.ItemsSourceProperty, parentLocations);
            ParentLocationListBox.DisplayMemberPath = "Name";
        }

        private void AddNewNameClick(object sender, RoutedEventArgs e) {
            Names.Add(AddNameTextbox.Text);
            (DataContext as Subdivision).Names = Names.ToList();
        }

        private void DeleteNameClick(object sender, RoutedEventArgs e) {
            Names.RemoveAt(AlternativeNamesListBox.Items.IndexOf(AlternativeNamesListBox.SelectedItem));
            (DataContext as Subdivision).Names = Names.ToList();
        }

        private void SelectParentClick(object sender, RoutedEventArgs e) {
            (DataContext as Subdivision).Parent = (GeopoliticalLocation)ParentLocationListBox.SelectedItem;
            (DataContext as Subdivision).SuperId = (int)(DataContext as Subdivision).Parent.Id;
        }

        private void LocationSearchTextChanged(object sender, TextChangedEventArgs e) {
            DisplayedLocations.Clear();
            foreach (GeopoliticalLocation location in AllLocations.Where(location => location.Name.Contains((sender as TextBox).Text))) {
                DisplayedLocations.Add(location);
            }
        }
        private async void SubmitClick(object sender, RoutedEventArgs e) {
            (DataContext as Subdivision).Names = Names.ToList();
            await DAO.Geography.Update((Subdivision)DataContext);
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
