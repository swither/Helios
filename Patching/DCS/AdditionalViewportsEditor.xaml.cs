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
    using GadrocsWorkshop.Helios.Windows.Controls;
    using System.Collections.Generic;
    using System.Windows;

    /// <summary>
    /// This interface editor manages a collection of DCS installation locations and allows installation of viewport patches into those locations.
    /// </summary>
    public partial class AdditionalViewportsEditor : HeliosInterfaceEditor
    {
        /// <summary>
        /// map from path to status
        /// </summary>
        private Dictionary<string, DestinationPatches> _destinations = new Dictionary<string, DestinationPatches>();

        public AdditionalViewportsEditor()
        {
            InitializeComponent();

            // load patches for all destinations
            InstallationLocations locations = DCS.InstallationLocations.Singleton;
            foreach (InstallationLocation location in locations.Items)
            {
                _destinations[location.Path] = new DestinationPatches(location, "Viewports");
            }

            // check if all selected patches are installed
            foreach (DestinationPatches status in _destinations.Values)
            {
                status.CheckApplied();
            }

            // update overall status
            UpdateStatus();

            // register for changes in selected destinations so we can scan again
            locations.Added += OnAdded;
            locations.Removed += OnRemoved;
            locations.Enabled += OnEnabled;
            locations.Disabled += OnDisabled;
        }

        private void OnDisabled(object sender, InstallationLocations.LocationEvent e)
        {
            _destinations[e.Location.Path].Enabled = false;
            UpdateStatus();
        }

        private void OnEnabled(object sender, InstallationLocations.LocationEvent e)
        {
            DestinationPatches destinationPatches = _destinations[e.Location.Path];
            destinationPatches.Enabled = true;
            destinationPatches.CheckApplied();
            UpdateStatus();
        }

        private void OnRemoved(object sender, InstallationLocations.LocationEvent e)
        {
            _destinations.Remove(e.Location.Path);
            UpdateStatus();
        }

        private void OnAdded(object sender, InstallationLocations.LocationEvent e)
        {
            DestinationPatches destinationStatus = new DestinationPatches(e.Location, "Viewports");
            destinationStatus.CheckApplied();
            _destinations[e.Location.Path] = destinationStatus;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            StatusCodes newStatus = StatusCodes.UpToDate;
            foreach (DestinationPatches status in _destinations.Values)
            {
                if (!status.Enabled)
                {
                    // does not count
                    continue;
                }
                switch (status.Status)
                {
                    case StatusCodes.Unknown:
                    case StatusCodes.OutOfDate:
                        newStatus = StatusCodes.OutOfDate;
                        // don't end iteration, need to check for failures
                        break;
                    case StatusCodes.UpToDate:
                        break;
                    case StatusCodes.Incompatible:
                        // any destination being incompatible counts
                        Status = StatusCodes.Incompatible;
                        return;
                }
            }
            Status = newStatus;
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
            }
            else
            {
                // XXX deinit; provoke crash on attempt to use 
            }
            // XXX need to rebind everything on the form
        }

        #region Commands
        private void Configure_Click(object sender, RoutedEventArgs e)
        {
            List<StatusReportItem> results = new List<StatusReportItem>();
            bool failed = false;
            bool imprecise = false;
            string message = "";

            // simulate patches and collect any errors
            foreach (DestinationPatches item in _destinations.Values)
            {
                foreach (StatusReportItem result in item.Patches.SimulateApply(item.Destination))
                {
                    result.Log(ConfigManager.LogManager);
                    results.Add(result);
                    if (result.Severity >= StatusReportItem.SeverityCode.Error)
                    {
                        if (!failed)
                        {
                            // keep first message
                            message = $"{result.Status}\n{result.Recommendation}";
                        }
                        failed = true;
                        item.Status = StatusCodes.Incompatible;
                    }
                    if (result.Severity >= StatusReportItem.SeverityCode.Warning)
                    {
                        imprecise = true;
                    }
                }
            }

            if (failed)
            {
                UpdateStatus();
                MessageBox.Show(Window.GetWindow(this), message, "Helios Viewport patch installation would fail");
                return;
            }

            if (imprecise)
            {
                MessageBoxResult result = MessageBox.Show(Window.GetWindow(this), "Installation of the viewport patches for DCS can continue, but some DCS files have changed since these patches were created.", "Helios Viewport patch installation may have risks", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel)
                {
                    // status unchanged
                    return;
                }
            }

            // apply patches
            foreach (DestinationPatches item in _destinations.Values)
            {
                StatusCodes newStatus = StatusCodes.UpToDate;
                foreach (StatusReportItem result in item.Patches.Apply(item.Destination))
                {
                    result.Log(ConfigManager.LogManager);
                    results.Add(result);
                    if (result.Severity >= StatusReportItem.SeverityCode.Error)
                    {
                        if (!failed)
                        {
                            // keep first message
                            message = $"{result.Status}\n{result.Recommendation}";
                        }
                        failed = true;
                        newStatus = StatusCodes.Incompatible;
                    }
                }
                item.Status = newStatus;
            }

            // message box the result if failed
            // XXX need to revert any patches that were installed, if we can, and add to result report
            // XXX add a custom dialog to show the detailed report of "results"
            UpdateStatus();
            if (failed)
            {
                MessageBox.Show(Window.GetWindow(this), message, "Helios Viewport patch installation failed");
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            // XXX revert patches where applicable
        }
        #endregion

        #region Properties
        /// <summary>
        /// Patch installation status
        /// </summary>
        public StatusCodes Status
        {
            get { return (StatusCodes)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(StatusCodes), typeof(AdditionalViewportsEditor), new PropertyMetadata(StatusCodes.OutOfDate));
        #endregion
    }
}
