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
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;

    /// <summary>
    /// This interface editor manages a collection of DCS installation locations and allows installation of viewport patches into those locations.
    /// 
    /// It also translates from DCS-specific installation location to generic patching interfaces to be shared with other instances of patching things
    /// </summary>
    public partial class MonitorSetupEditor : HeliosInterfaceEditor, IStatusReportObserver
    {
        private InstallationDialogs _installationDialogs;
        private MonitorSetup _parent;
        private InstallationLocations _locations;

        public MonitorSetupEditor()
        {
            DataContext = this;
            InitializeComponent();

            // load patches for all destinations
            Dictionary<string, PatchDestinationViewModel> destinations = new Dictionary<string, PatchDestinationViewModel>();
            _locations = DCS.InstallationLocations.Singleton;
            foreach (InstallationLocation location in _locations.Items)
            {
                destinations[location.Path] = new PatchDestinationViewModel(location, AdditionalViewports.PATCH_SET);
            }

            // register for changes in selected destinations because these invalidate our status
            _locations.Added += OnLocationAdded;
            _locations.Removed += OnLocationRemoved;
            _locations.Enabled += OnLocationEnabled;
            _locations.Disabled += OnLocationDisabled;

            _installationDialogs = new InstallationDialogs(this);
        }

        private void OnLocationDisabled(object sender, InstallationLocations.LocationEvent e)
        {
            _parent?.InvalidateStatusReport();
        }

        private void OnLocationEnabled(object sender, InstallationLocations.LocationEvent e)
        {
            _parent?.InvalidateStatusReport();
        }

        private void OnLocationRemoved(object sender, InstallationLocations.LocationEvent e)
        {
            _parent?.InvalidateStatusReport();
        }

        private void OnLocationAdded(object sender, InstallationLocations.LocationEvent e)
        {
            _parent?.InvalidateStatusReport();
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
                _parent = monitorSetupInterface;

                // initialize our monitors and viewports and calculate bounds
                Monitors = _parent.Monitors;
                Viewports = _parent.Viewports;
                OnGeometryChange();

                // sign up for changes from property changes and from ready check
                _parent.GeometryChange += Parent_GeometryChange;
                _parent.Subscribe(this);
                _parent?.InvalidateStatusReport();
            }
            else
            {
                // deinit; provoke crash on attempt to use
                Dispose();
            }
        }

        private void Parent_GeometryChange(object sender, EventArgs e)
        {
            // some monitor or viewport has changed, recalculate bounds
            OnGeometryChange();
        }

        private void OnGeometryChange()
        {
            Rect totalBounds = new Rect(0, 0, 1, 1);
            Rect bounds = new Rect(0,0,1,1);
            Rect rawBounds = new Rect(0, 0, 1, 1);
            Rect mainBounds = new Rect(0, 0, 0, 0);
            Rect uiBounds = new Rect(0, 0, 0, 0);

            List<MonitorViewModel> mainMonitors = new List<MonitorViewModel>();
            List<MonitorViewModel> uiMonitors = new List<MonitorViewModel>();
            foreach (MonitorViewModel monitor in Monitors)
            {
                totalBounds.Union(monitor.Rect);
                if (monitor.Main)
                {
                    if (!monitor.Included)
                    {
                        monitor.Included = true;
                    }
                    if (monitor.CanBeExcluded)
                    {
                        // this change is necessary when we add the last monitor to main
                        monitor.CanBeExcluded = false;
                    }
                    mainMonitors.Add(monitor);
                    if (mainBounds.Width > 0)
                    {
                        mainBounds.Union(monitor.Rect);
                    }
                    else
                    {
                        // these are structs, so this copies
                        mainBounds = monitor.Rect;
                    }
                }
                if (monitor.UserInterface)
                {
                    if (!monitor.Included)
                    {
                        monitor.Included = true;
                    }
                    if (monitor.CanBeExcluded)
                    {
                        // this change is necessary when we add the last monitor to UI
                        monitor.CanBeExcluded = false;
                    }
                    uiMonitors.Add(monitor);
                    if (uiBounds.Width > 0)
                    {
                        uiBounds.Union(monitor.Rect);
                    }
                    else
                    {
                        // these are structs, so this copies
                        uiBounds = monitor.Rect;
                    }
                }
                if (!monitor.Included)
                {
                    continue;
                }
                bounds.Union(monitor.Rect);
                rawBounds.Union(monitor.RawRect);
            }
            ScaledResolutionWidth = bounds.Width;
            ScaledResolutionHeight = bounds.Height;
            ScaledTotalWidth = totalBounds.Width;
            ScaledTotalHeight = totalBounds.Height;
            ScaledMain = mainBounds;
            ScaledUserInterface = uiBounds;
            ResolutionWidth = rawBounds.Width;
            ResolutionHeight = rawBounds.Height;
            
            // now figure out ordering of monitors and enable the correct controls
            List<MonitorViewModel> index = Monitors.OrderBy(v => -v.Rect.Right).ToList();

            // don't allow the left-most monitor to be excluded
            int leftMost = index.Count - 1;
            index[leftMost].CanBeExcluded = false;
            index[leftMost].Included = true;
            index.RemoveAt(leftMost);

            // now fix up the flags for all the other monitors, which may have moved around
            bool enableRest = false;
            foreach (MonitorViewModel view in index)
            {
                if (enableRest)
                {
                    // all monitors to the left must be included
                    view.Included = true;
                    view.CanBeExcluded = false;
                    continue;
                }

                // right-most monitors can be excluded unless it has an assigned function
                // XXX check for viewports
                if (!view.Main && !view.UserInterface)
                {
                    view.CanBeExcluded = true;
                }

                if (view.Included)
                {
                    // found one that is the first included monitor
                    enableRest = true;
                }
            }

            if (mainMonitors.Count == 1)
            {            
                // can't remove the last main view
                mainMonitors[0].CanBeRemovedFromMain = false;
            }
            else
            {
                mainMonitors.ForEach(m => m.CanBeRemovedFromMain = true);
            }
        }

        private void Dispose()
        {
            if (_locations != null)
            {
                _locations.Added -= OnLocationAdded;
                _locations.Removed -= OnLocationRemoved;
                _locations.Enabled -= OnLocationEnabled;
                _locations.Disabled -= OnLocationDisabled;
                _locations = null;
            }
            if (_parent != null)
            {
                _parent.Unsubscribe(this);
                _parent = null;
            }
        }

        private void OnStatusChange(object sender, EventArgs e)
        {
            _parent?.InvalidateStatusReport();
        }

        #region Commands
        private void Configure_Click(object sender, RoutedEventArgs e)
        {
            _parent?.Configure(_locations, _installationDialogs);
            _parent?.InvalidateStatusReport();
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

        public void ReceiveStatusReport(IEnumerable<StatusReportItem> statusReport)
        {
            // figure out our UI status
            StatusCodes newStatus = StatusCodes.Unknown;
            if (!_parent.Profile.IsValidMonitorLayout)
            {
                newStatus = StatusCodes.ResetMonitorsRequired;
            }
            else if (_locations.Items.Count == 0)
            {
                newStatus = StatusCodes.NoLocations;
            }
            else
            {
                // run regular ready check to see if any items aren't "configuration up to date"
                if (statusReport.Any(item => !item.Flags.HasFlag(StatusReportItem.StatusFlags.ConfigurationUpToDate)))
                {
                    newStatus = StatusCodes.OutOfDate;
                }
                else
                {
                    newStatus = StatusCodes.UpToDate;
                }
            }
            Status = newStatus;
        }
        #endregion

        #region Properties
        public StatusCodes Status
        {
            get { return (StatusCodes)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(StatusCodes), typeof(MonitorSetupEditor), new PropertyMetadata(StatusCodes.Unknown));

        public ObservableCollection<MonitorViewModel> Monitors
        {
            get { return (ObservableCollection<MonitorViewModel>)GetValue(MonitorsProperty); }
            set { SetValue(MonitorsProperty, value); }
        }
        public static readonly DependencyProperty MonitorsProperty =
            DependencyProperty.Register("Monitors", typeof(ObservableCollection<MonitorViewModel>), typeof(MonitorSetupEditor), new PropertyMetadata(null));

        public ObservableCollection<ViewportViewModel> Viewports
        {
            get { return (ObservableCollection<ViewportViewModel>)GetValue(ViewportsProperty); }
            set { SetValue(ViewportsProperty, value); }
        }
        public static readonly DependencyProperty ViewportsProperty =
            DependencyProperty.Register("Viewports", typeof(ObservableCollection<ViewportViewModel>), typeof(MonitorSetupEditor), new PropertyMetadata(null));

        public double ScaledTotalWidth
        {
            get { return (double)GetValue(ScaledTotalWidthProperty); }
            set { SetValue(ScaledTotalWidthProperty, value); }
        }
        public static readonly DependencyProperty ScaledTotalWidthProperty =
            DependencyProperty.Register("ScaledTotalWidth", typeof(double), typeof(MonitorSetupEditor), new PropertyMetadata(1.0));

        public double ScaledTotalHeight
        {
            get { return (double)GetValue(ScaledTotalHeightProperty); }
            set { SetValue(ScaledTotalHeightProperty, value); }
        }
        public static readonly DependencyProperty ScaledTotalHeightProperty =
            DependencyProperty.Register("ScaledTotalHeight", typeof(double), typeof(MonitorSetupEditor), new PropertyMetadata(1.0));

        public double ScaledResolutionWidth
        {
            get { return (double)GetValue(ScaledResolutionWidthProperty); }
            set { SetValue(ScaledResolutionWidthProperty, value); }
        }
        public static readonly DependencyProperty ScaledResolutionWidthProperty =
            DependencyProperty.Register("ScaledResolutionWidth", typeof(double), typeof(MonitorSetupEditor), new PropertyMetadata(1.0));

        public double ScaledResolutionHeight
        {
            get { return (double)GetValue(ScaledResolutionHeightProperty); }
            set { SetValue(ScaledResolutionHeightProperty, value); }
        }
        public static readonly DependencyProperty ScaledResolutionHeightProperty =
            DependencyProperty.Register("ScaledResolutionHeight", typeof(double), typeof(MonitorSetupEditor), new PropertyMetadata(1.0));

        public double ResolutionWidth
        {
            get { return (double)GetValue(ResolutionWidthProperty); }
            set { SetValue(ResolutionWidthProperty, value); }
        }
        public static readonly DependencyProperty ResolutionWidthProperty =
            DependencyProperty.Register("ResolutionWidth", typeof(double), typeof(MonitorSetupEditor), new PropertyMetadata(10.0));

        public double ResolutionHeight
        {
            get { return (double)GetValue(ResolutionHeightProperty); }
            set { SetValue(ResolutionHeightProperty, value); }
        }
        public static readonly DependencyProperty ResolutionHeightProperty =
            DependencyProperty.Register("ResolutionHeight", typeof(double), typeof(MonitorSetupEditor), new PropertyMetadata(10.0));

        public Rect ScaledMain
        {
            get { return (Rect)GetValue(ScaledMainProperty); }
            set { SetValue(ScaledMainProperty, value); }
        }
        public static readonly DependencyProperty ScaledMainProperty =
            DependencyProperty.Register("ScaledMain", typeof(Rect), typeof(MonitorSetupEditor), new PropertyMetadata(null));

        public Rect ScaledUserInterface
        {
            get { return (Rect)GetValue(ScaledUserInterfaceProperty); }
            set { SetValue(ScaledUserInterfaceProperty, value); }
        }
        public static readonly DependencyProperty ScaledUserInterfaceProperty =
            DependencyProperty.Register("ScaledUserInterface", typeof(Rect), typeof(MonitorSetupEditor), new PropertyMetadata(null));

        #endregion
    }
}
