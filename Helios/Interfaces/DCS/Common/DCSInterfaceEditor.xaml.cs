//  Copyright 2014 Craig Courtney
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    using GadrocsWorkshop.Helios.Windows.Controls;
    using System;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for DCSInterfaceEditor.xaml
    /// 
    /// This DCS Interface editor can be used by descendants of DCSInterface that do not want to add any specific options.
    /// Using this class will avoid duplicating the XAML.
    /// 
    /// TODO: implement a content container into which specific options can be added.
    /// </summary>
    public partial class DCSInterfaceEditor : HeliosInterfaceEditor
    {
        // sub-configuration objects we path into
        protected DCSExportConfiguration _configuration;
        protected DCSPhantomMonitorFixConfig _phantomFix;
        protected DCSVehicleImpersonation _vehicleImpersonation;

        static DCSInterfaceEditor()
        {
            Type ownerType = typeof(DCSInterfaceEditor);
            CommandManager.RegisterClassCommandBinding(ownerType, new CommandBinding(AddDoFileCommand, AddDoFile_Executed));
            CommandManager.RegisterClassCommandBinding(ownerType, new CommandBinding(RemoveDoFileCommand, RemoveDoFile_Executed));
        }

        public DCSInterfaceEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called immediately after construction when our factory installs the Interface property, not
        /// executed at run time (Profile Editor only)
        /// </summary>
        protected override void OnInterfaceChanged(HeliosInterface oldInterface, HeliosInterface newInterface)
        {
            base.OnInterfaceChanged(oldInterface, newInterface);
            if (newInterface is DCSInterface dcsInterface) {
                // create configuration objects
                _configuration = new DCSExportConfiguration(dcsInterface);
                _phantomFix = new DCSPhantomMonitorFixConfig(dcsInterface);
                _vehicleImpersonation = new DCSVehicleImpersonation(dcsInterface);

                // calculate up to date status
                _configuration.UpdateExports();
            }
            else {
                // provoke crash on attempt to use 
                _configuration = null;
                _phantomFix = null;
                _vehicleImpersonation = null;
            }
            // need to rebind everything on the form
            SetValue(ConfigurationProperty, _configuration);
            SetValue(PhantomFixProperty, _phantomFix);
            SetValue(VehicleImpersonationProperty, _vehicleImpersonation);
        }

        #region Commands
        public static readonly RoutedUICommand AddDoFileCommand = new RoutedUICommand("Adds a dofile(...) to a DCS config.", "AddDoFile", typeof(DCSInterfaceEditor));
        public static readonly RoutedUICommand RemoveDoFileCommand = new RoutedUICommand("Removes a dofile(...) to a DCS config.", "RemoveDoFile", typeof(DCSInterfaceEditor));

        private static void AddDoFile_Executed(object target, ExecutedRoutedEventArgs e)
        {
            DCSInterfaceEditor editor = target as DCSInterfaceEditor;
            string file = e.Parameter as string;
            if (editor != null && !string.IsNullOrWhiteSpace(file) && !editor._configuration.DoFiles.Contains(file))
            {
                editor._configuration.DoFiles.Add((string)e.Parameter);
                editor.NewDoFile.Text = "";
            }
        }

        private static void RemoveDoFile_Executed(object target, ExecutedRoutedEventArgs e)
        {
            DCSInterfaceEditor editor = target as DCSInterfaceEditor;
            string file = e.Parameter as string;
            if (editor != null && !string.IsNullOrWhiteSpace(file) && editor._configuration.DoFiles.Contains(file))
            {
                editor._configuration.DoFiles.Remove(file);
            }
        }

        private void Configure_Click(object sender, RoutedEventArgs e)
        {
            if (_configuration.WriteExports())
            {
                MessageBox.Show(Window.GetWindow(this), $"DCS export scripts for {Interface.Name} have been configured.");
            }
            else
            {
                MessageBox.Show(Window.GetWindow(this), $"Error updating DCS export scripts for {Interface.Name}.");
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            // XXX consider removing this _configuration.RestoreConfig();
        }
        #endregion

        #region Properties
        /// <summary>
        /// used by UI binding paths.
        /// </summary>
        public DCSExportConfiguration Configuration { get => (DCSExportConfiguration)GetValue(ConfigurationProperty); }
        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register("Configuration", typeof(DCSExportConfiguration), typeof(DCSInterfaceEditor), new PropertyMetadata(null));

        /// <summary>
        /// used by UI binding paths
        /// </summary>
        public DCSPhantomMonitorFixConfig PhantomFix { get => (DCSPhantomMonitorFixConfig)GetValue(PhantomFixProperty); }
        public static readonly DependencyProperty PhantomFixProperty =
            DependencyProperty.Register("PhantomFix", typeof(DCSPhantomMonitorFixConfig), typeof(DCSInterfaceEditor), new PropertyMetadata(null));

        /// <summary>
        /// used by UI binding paths
        /// </summary>
        public DCSVehicleImpersonation VehicleImpersonation { get => (DCSVehicleImpersonation)GetValue(VehicleImpersonationProperty); }
        public static readonly DependencyProperty VehicleImpersonationProperty =
            DependencyProperty.Register("VehicleImpersonation", typeof(DCSVehicleImpersonation), typeof(DCSInterfaceEditor), new PropertyMetadata(null));
        #endregion
    }
}