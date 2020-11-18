﻿using System;
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
using Model.Geographic;
using QWest.DataAcess;

namespace QWest.Admin {
    /// <summary>
    /// Interaction logic for EditCountry.xaml
    /// </summary>
    public partial class EditCountry : Window {
        public EditCountry(Country country) {
            InitializeComponent();
            DataContext = country;
        }

        private async void Submit_Click(object sender, RoutedEventArgs e) {
            await DAO.Geography.Update((Country)DataContext);
            Close();
        }
    }
}
