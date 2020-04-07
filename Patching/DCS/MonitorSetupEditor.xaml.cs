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

using System.Windows.Controls;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    using GadrocsWorkshop.Helios;
    using GadrocsWorkshop.Helios.Windows;
    using GadrocsWorkshop.Helios.Windows.Controls;
    using System.Windows;

    /// <summary>
    /// This interface editor allows generation of a DCS Monitor Setup by presenting a view of installed monitors
    /// and extra viewports that have been placed.  By manipulating this graphically, a valid DCS monitor configuration
    /// can be generated.
    /// </summary>
    public partial class MonitorSetupEditor : HeliosInterfaceEditor
    {
        private readonly InstallationDialogs _installationDialogs;
        private MonitorSetupViewModel _model;

        public MonitorSetupEditor()
        {
            InitializeComponent();
            _installationDialogs = new InstallationDialogs(this);
        }

        /// <summary>
        /// Called immediately after construction when our factory installs the Interface property, not
        /// executed at run time (Profile Editor only)
        /// </summary>
        protected override void OnInterfaceChanged(HeliosInterface oldInterface, HeliosInterface newInterface)
        {
            base.OnInterfaceChanged(oldInterface, newInterface);
            if (newInterface is MonitorSetup monitorSetupInterface)
            {
                _model = new MonitorSetupViewModel(monitorSetupInterface);
                DataContext = _model;
            }
            else
            {
                // deinit; provoke crash on attempt to use
                Dispose();
            }
        }

        // XXX eliminate if we dont need an events
        private void Dispose()
        {
            _model = null;
            DataContext = null;
        }

#region Commands
        private void Configure_Click(object sender, RoutedEventArgs e)
        {
            _model?.Data.Install(_installationDialogs);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            // XXX revert patches where applicable
        }

        public override void Closed()
        {
            Dispose();
            base.Closed();
        }

#endregion


    }
}
