using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GadrocsWorkshop.Helios.ProfileEditor
{
    /// <summary>
    /// Interaction logic for Checklist.xaml
    /// </summary>
    public partial class Checklist : Grid
    {
        #region Commands
        public static RoutedUICommand GoThereCommand { get; } = new RoutedUICommand("Opens an associated editor.", "GoThere", typeof(Checklist));
        #endregion

        public Checklist()
        {
            InitializeComponent();
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            // XXX PerformChecks();
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
