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

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    using GadrocsWorkshop.Helios;
    using GadrocsWorkshop.Helios.Windows.Controls;
    using System.Collections.Generic;
    using System.Windows;

    /// <summary>
    /// This interface editor manages a collection of DCS installation locations and allows installation of viewport patches into those locations.
    /// 
    /// It also translates from DCS-specific installation location to generic patching interfaces to be shared with other instances of patching things
    /// </summary>
    public partial class AdditionalViewportsEditor : HeliosInterfaceEditor
    {
        private InstallationDialogs _installationDialogs;

        public AdditionalViewportsEditor()
        {
            InitializeComponent();

            // load patches for all destinations
            Dictionary<string, PatchDestinationViewModel> destinations = new Dictionary<string, PatchDestinationViewModel>();
            InstallationLocations locations = DCS.InstallationLocations.Singleton;
            foreach (InstallationLocation location in locations.Items)
            {
                destinations[location.Path] = new PatchDestinationViewModel(location, AdditionalViewports.PATCH_SET);
            }

            Patching = new PatchingViewModel(destinations, AdditionalViewports.PATCH_SET, "Helios viewport patches");

            // register for changes in selected destinations so we can scan again
            locations.Added += OnAdded;
            locations.Removed += OnRemoved;
            locations.Enabled += OnEnabled;
            locations.Disabled += OnDisabled;

            _installationDialogs = new InstallationDialogs(this);
        }

        private void OnDisabled(object sender, InstallationLocations.LocationEvent e)
        {
            Patching?.OnDisabled(e.Location.Path);
        }

        private void OnEnabled(object sender, InstallationLocations.LocationEvent e)
        {
            Patching?.OnEnabled(e.Location.Path);
        }

        private void OnRemoved(object sender, InstallationLocations.LocationEvent e)
        {
            Patching?.OnRemoved(e.Location.Path);
        }

        private void OnAdded(object sender, InstallationLocations.LocationEvent e)
        {
            PatchDestinationViewModel destinationPatches = new PatchDestinationViewModel(e.Location, AdditionalViewports.PATCH_SET);
            Patching?.OnAdded(e.Location.Path, destinationPatches);
        }

        /// <summary>
        /// Called immediately after construction when our factory installs the Interface property, not
        /// executed at run time (Profile Editor only)
        /// </summary>
        protected override void OnInterfaceChanged(HeliosInterface oldInterface, HeliosInterface newInterface)
        {
            base.OnInterfaceChanged(oldInterface, newInterface);
            if (newInterface is AdditionalViewports viewportsInterface)
            {
                // do things with our interface, if we need to
            }
            else
            {
                // deinit; provoke crash on attempt to use 
            }
        }

        #region Commands
        private void Configure_Click(object sender, RoutedEventArgs e)
        {
            Patching?.Install(_installationDialogs);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            // XXX revert patches where applicable
        }
        #endregion

        #region Properties
        public PatchingViewModel Patching
        {
            get { return (PatchingViewModel)GetValue(PatchingProperty); }
            set { SetValue(PatchingProperty, value); }
        }
        public static readonly DependencyProperty PatchingProperty =
            DependencyProperty.Register("Patching", typeof(PatchingViewModel), typeof(AdditionalViewportsEditor), new PropertyMetadata(null));
        #endregion
    }
}
