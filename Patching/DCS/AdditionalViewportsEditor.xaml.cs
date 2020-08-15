// Copyright 2020 Ammo Goettsch
// 
// Helios is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Helios is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.Windows;
using GadrocsWorkshop.Helios.Util.DCS;
using GadrocsWorkshop.Helios.Windows;
using GadrocsWorkshop.Helios.Windows.Controls;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    /// <summary>
    /// This interface editor manages a collection of DCS installation locations and allows installation of viewport patches
    /// into those locations.
    /// It also translates from DCS-specific installation location to generic patching interfaces to be shared with other
    /// instances of patching things
    /// </summary>
    public partial class AdditionalViewportsEditor : HeliosInterfaceEditor
    {
        private readonly InstallationDialogs _installationDialogs;
        private AdditionalViewports _parent;

        public AdditionalViewportsEditor()
        {
            InitializeComponent();

            InstallationLocations locations = InstallationLocations.Singleton;

            // load patches for all destinations
            Patching = new PatchingConfiguration(LoadDestinations(locations), AdditionalViewports.PATCH_SET,
                "Helios viewport patches");

            // register for changes in selected destinations so we can scan again
            locations.Added += OnAdded;
            locations.Removed += OnRemoved;
            locations.Enabled += OnEnabled;
            locations.Disabled += OnDisabled;
            locations.RemoteChanged += OnRemoteChanged;

            _installationDialogs = new InstallationDialogs(this);
        }

        private static Dictionary<string, PatchApplication> LoadDestinations(InstallationLocations locations)
        {
            Dictionary<string, PatchApplication> destinations =
                new Dictionary<string, PatchApplication>();

            if (locations.IsRemote)
            {
                // just leave it blank
                return destinations;
            }

            foreach (InstallationLocation location in locations.Items)
            {
                destinations[location.Path] = AdditionalViewports.CreatePatchDestination(location);
            }

            return destinations;
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
            Patching?.OnAdded(e.Location.Path, AdditionalViewports.CreatePatchDestination(e.Location));
        }

        private void OnRemoteChanged(object sender, EventArgs e)
        {
            Patching?.OnRemoteChanged(LoadDestinations(InstallationLocations.Singleton));
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
                _parent = viewportsInterface;
            }
            else
            {
                // deinit; provoke crash on attempt to use 
                _parent = null;
            }
        }

        #region Commands

        private void Configure_Click(object sender, RoutedEventArgs e)
        {
            Patching?.Install(_installationDialogs);
            _parent.InvalidateStatusReport();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            Patching?.Uninstall(_installationDialogs);
            _parent.InvalidateStatusReport();
        }

        #endregion

        #region Properties

        public PatchingConfiguration Patching
        {
            get => (PatchingConfiguration) GetValue(PatchingProperty);
            set => SetValue(PatchingProperty, value);
        }

        public static readonly DependencyProperty PatchingProperty =
            DependencyProperty.Register("Patching", typeof(PatchingConfiguration), typeof(AdditionalViewportsEditor),
                new PropertyMetadata(null));

        #endregion
    }
}