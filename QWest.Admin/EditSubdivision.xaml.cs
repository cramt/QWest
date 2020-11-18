﻿using Model.Geographic;
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
        public EditSubdivision(Subdivision subdivision) {
            InitializeComponent();
            DataContext = subdivision;

            if (subdivision.Names != null) {
                Names = new ObservableCollection<string>(subdivision.Names);
            }
            else {
                Names = new ObservableCollection<string>();
            }
            AlternativeNamesListBox.DataContext = this;
        }

        private void AddNewNameClick(object sender, RoutedEventArgs e) {
            Names.Add(AddNameTextbox.Text);
            (DataContext as Country).Names = Names.ToList();
        }

        private void DeleteNameClick(object sender, RoutedEventArgs e) {
            Names.RemoveAt(AlternativeNamesListBox.Items.IndexOf(AlternativeNamesListBox.SelectedItem));
            (DataContext as Country).Names = Names.ToList();
        }
        private async void SubmitClick(object sender, RoutedEventArgs e) {
            (DataContext as Country).Names = Names.ToList();
            await DAO.Geography.Update((Country)DataContext);
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
