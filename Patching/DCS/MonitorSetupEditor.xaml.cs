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
    using GadrocsWorkshop.Helios.Util;
    using GadrocsWorkshop.Helios.Util.DCS;
    using GadrocsWorkshop.Helios.Windows.Controls;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    /// This interface editor allows generation of a DCS Monitor Setup by presenting a view of installed monitors
    /// and extra viewports that have been placed.  By manipulating this graphically, a valid DCS monitor configuration
    /// can be generated.
    /// </summary>
    public partial class MonitorSetupEditor : HeliosInterfaceEditor, IStatusReportObserver
    {
        private InstallationDialogs _installationDialogs;
        private MonitorSetup _parent;
        private InstallationLocations _locations;
        private DispatcherTimer _geometryChangeTimer;

        public MonitorSetupEditor()
        {
            DataContext = this;
            InitializeComponent();

            // load patches for all destinations
            Dictionary<string, PatchDestinationViewModel> destinations = new Dictionary<string, PatchDestinationViewModel>();
            _locations = InstallationLocations.Singleton;
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
                _geometryChangeTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Normal, OnGeometryChange, Dispatcher);

                // initialize our monitors and viewports and calculate bounds
                Monitors = _parent.Monitors;
                Viewports = _parent.Viewports;
                _geometryChangeTimer.Start();

                // sign up for changes from property changes and from ready check
                _parent.GeometryChange += Parent_GeometryChange;
                _parent.Subscribe(this);
                _parent.InvalidateStatusReport();
            }
            else
            {
                // deinit; provoke crash on attempt to use
                Dispose();
            }
        }

        private void Parent_GeometryChange(object sender, EventArgs e)
        {
            // eat all events for a short duration and process only once in case there are a lot of updates
            _geometryChangeTimer?.Start();
        }

        private void OnGeometryChange(object sender, EventArgs e)
        {
            _geometryChangeTimer.Stop();
            List<MonitorViewModel> mainMonitors, uiMonitors;
            ClassifyMonitorsAndUpdateBounds(out mainMonitors, out uiMonitors);
            OnlyAllowRightMostMonitorRemoved();
            ProtectLastMonitor(mainMonitors, (m, value) => m.CanBeRemovedFromMain = value);
            ProtectLastMonitor(uiMonitors, (m, value) => m.CanBeRemovedFromUserInterface = value);

            // also request new status report
            _parent?.InvalidateStatusReport();
        }

        // XXX this is all wrong, all of this belongs in the model (code in MonitorSetup operating on ShadowMonitor) once we refactor
        private void ClassifyMonitorsAndUpdateBounds(out List<MonitorViewModel> mainMonitors, out List<MonitorViewModel> uiMonitors)
        {
            Rect totalBounds = new Rect(0, 0, 1, 1);
            Rect bounds = new Rect(0, 0, 1, 1);
            Rect rawBounds = new Rect(0, 0, 1, 1);
            Rect mainBounds = new Rect(0, 0, 0, 0);
            Rect uiBounds = new Rect(0, 0, 0, 0);

            mainMonitors = new List<MonitorViewModel>();
            uiMonitors = new List<MonitorViewModel>();
            foreach (MonitorViewModel monitor in Monitors)
            {
                totalBounds.Union(monitor.Rect);
                if (monitor.HasContent)
                {
                    monitor.SetCanExclude(false, "This monitor must be included in the area drawn by DCS because there is content on it.");
                }
                if (monitor.Main)
                {
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
        }

        private void OnlyAllowRightMostMonitorRemoved()
        {
            // now figure out ordering of monitors and enable the correct controls
            List<MonitorViewModel> index = Monitors.OrderBy(v => -v.Rect.Right).ToList();

            // don't allow the left-most monitor to be excluded
            int leftMost = index.Count - 1;
            index[leftMost].SetCanExclude(false, "This monitor can not be unselected because DCS requires drawing on the left-most monitor.");
            index.RemoveAt(leftMost);

            // now fix up the flags for all the other monitors, which may have moved around
            bool enableRest = false;
            foreach (MonitorViewModel monitor in index)
            {
                if (enableRest)
                {
                    // all monitors to the left must be included
                    monitor.SetCanExclude(false, "This monitor can not be unselected because there are selected monitors to the right of it.");
                    continue;
                }

                // right-most monitors can be excluded unless it has an assigned function
                if (!monitor.HasContent)
                {
                    monitor.SetCanExclude(true, "This monitor can be removed from the area that will be drawn by DCS because it is the right-most monitor and it is empty.");
                }

                if (monitor.Included)
                {
                    // found one that is the first included monitor
                    enableRest = true;
                }
            }
        }

        private static void ProtectLastMonitor(List<MonitorViewModel> monitors, Action<MonitorViewModel, bool> setter)
        {
            if (monitors.Count == 1)
            {
                // can't remove the last main view
                setter(monitors[0], false);
            }
            else
            {
                monitors.ForEach(m => setter(m, true));
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

        #region Commands
        private void Configure_Click(object sender, RoutedEventArgs e)
        {
            _parent?.Install(_installationDialogs);
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
            else if (_locations.Items.Where(l => l.IsEnabled).Count() < 1)
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
