// Copyright 2021 Ammo Goettsch
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using GadrocsWorkshop.Helios.Interfaces.Capabilities;
using GadrocsWorkshop.Helios.Util.Shadow;

namespace GadrocsWorkshop.Helios.Interfaces.Common
{
    /// <summary>
    /// a Helios interface that monitors any viewport extents and tracks which monitors they fall on, in order to create
    /// configuration
    /// output based on the true screen coordinates of those viewports
    /// </summary>
    /// <typeparam name="TMonitor"></typeparam>
    /// <typeparam name="TMonitorEventArgs"></typeparam>
    public abstract class ViewportCompilerInterface<TMonitor, TMonitorEventArgs> : HeliosInterface, IReadyCheck,
        IStatusReportNotify,
        IShadowVisualParent, IInstallation, IResetMonitorsObserver,
        IExtendedDescription where TMonitorEventArgs : EventArgs where TMonitor : ShadowMonitorBase<TMonitorEventArgs>
    {
        /// <summary>
        /// fired when the bounds of some view model items may have changed, delayed to batch many changes
        /// </summary>
        public virtual event EventHandler GeometryChangeDelayed;

        /// <summary>
        /// fired when the offset of the top left corner from Windows screen coordinates to
        /// (0,0)-based coordinates has changed
        /// </summary>
        public virtual event EventHandler<GlobalOffsetEventArgs> GlobalOffsetChanged;

        public virtual event EventHandler<TMonitorEventArgs> MonitorAdded;
        public virtual event EventHandler<TMonitorEventArgs> MonitorRemoved;

        public virtual event EventHandler<ShadowViewportEventArgs> ViewportAdded;
        public virtual event EventHandler<ShadowViewportEventArgs> ViewportRemoved;

        /// <summary>
        /// live inventory of our profile, indexed by Helios monitor object
        /// </summary>
        protected readonly Dictionary<Monitor, TMonitor> _monitors = new Dictionary<Monitor, TMonitor>();

        /// <summary>
        /// timer to delay execution of change in geometry because we sometimes receive thousands of events,
        /// such as on reset monitors
        /// </summary>
        protected DispatcherTimer _geometryChangeTimer;

        /// <summary>
        /// if true, then we are currently processing a geometry change and making changes that could schedule
        /// further geometry change timer calls, so we should suppress those
        /// </summary>
        protected bool _geometryChanging;

        /// <summary>
        /// if true, then monitors are either invalid or currently being reset, so we should not do any automatic configurations
        /// </summary>
        protected bool _monitorsValid;

        private readonly HashSet<IStatusReportObserver> _observers = new HashSet<IStatusReportObserver>();

        /// <summary>
        /// if true, we are tracking visuals
        /// </summary>
        protected bool _started;

        /// <summary>
        /// live inventory of our profile, indexed by source visual
        /// </summary>
        private readonly Dictionary<HeliosVisual, ShadowVisual> _viewports =
            new Dictionary<HeliosVisual, ShadowVisual>();

        /// <summary>
        /// scale used for the viewmodels
        /// </summary>
        private double _scale = 0.1;

        /// <summary>
        /// backing field for property CurrentProfileName, contains
        /// short name of profile currently being edited
        /// </summary>
        private string _currentProfileName;

        protected ViewportCompilerInterface(string name) : base(name)
        {
            // don't do any work here since we don't have a profile yet and we may just be
            // test instantiated for add interface dialog        }
        }

        protected void CreateShadowObjects()
        {
            // recursively walk profile and track every visual
            foreach (Monitor monitor in Profile.Monitors)
            {
                AddMonitor(monitor);
            }
        }

        protected void ClearShadowObjects()
        {
            // clean up shadow collections
            foreach (TMonitor shadow in _monitors.Values)
            {
                shadow.Dispose();
                MonitorRemoved?.Invoke(this, shadow.CreateEvent());
            }

            foreach (ShadowVisual shadow in _viewports.Values)
            {
                shadow.Dispose();
                ViewportRemoved?.Invoke(this, new ShadowViewportEventArgs(shadow));
            }

            _monitors.Clear();
            _viewports.Clear();
        }

        /// <summary>
        /// find the offset between windows coordinates and the 0,0 coordinate system used by DCS
        /// </summary>
        protected void UpdateGlobalOffset()
        {
            if (!_monitors.Values.Any())
            {
                // can happen during reset
                return;
            }
            double globalX = -_monitors.Values.Select(m => m.Visual.Left).Min<double>();
            double globalY = -_monitors.Values.Select(m => m.Visual.Top).Min<double>();
            GlobalOffset = new Vector(globalX, globalY);
            ConfigManager.LogManager.LogDebug(
                $"new top left corner offset to translate from windows coordinates to DCS coordinates is {GlobalOffset}");

            // push this value to all viewports and monitors
            GlobalOffsetChanged?.Invoke(this, new GlobalOffsetEventArgs(GlobalOffset));
        }

        protected void ScheduleGeometryChange()
        {
            if (_geometryChanging)
            {
                // we are the source of whatever change caused us to call this, don't recurse
                return;
            }

            // eat all events for a short duration and process only once in case there are a lot of updates
            _geometryChangeTimer?.Start();
        }

        protected void StartShadowing()
        {
            if (_started)
            {
                return;
            }
            _started = true;

            // instrument all current monitors and visuals
            CreateShadowObjects();

            // we only update our models if the monitor layout matches
            _monitorsValid = CheckMonitorsValid;

            // calculate initial geometry, if we can
            UpdateAllGeometry();
        }

        protected void StopShadowing()
        {
            _geometryChangeTimer?.Stop();
            ClearShadowObjects();
            _started = false;
        }

        private void AddMonitor(Monitor monitor)
        {
            TMonitor shadow = CreateShadowMonitor(monitor);
            _monitors[monitor] = shadow;

            // now that we can find the monitor in our index, we can safely add viewports
            // and other children
            shadow.Instrument();
            shadow.PropertyChanged += Shadow_PropertyChanged;
            shadow.MonitorChanged += Raw_MonitorChanged;

            MonitorAdded?.Invoke(this, shadow.CreateEvent());
        }

        private void RemoveMonitor(Monitor monitor)
        {
            TMonitor shadow = _monitors[monitor];
            shadow.MonitorChanged -= Raw_MonitorChanged;
            MonitorRemoved?.Invoke(this, shadow.CreateEvent());
            _monitors.Remove(monitor);
            shadow.Dispose();
        }

        #region Hooks

        protected abstract TMonitor CreateShadowMonitor(Monitor monitor);

        protected abstract List<StatusReportItem> CreateStatusReport();
        public abstract InstallationResult Install(IInstallationCallbacks callbacks);

        public abstract IEnumerable<StatusReportItem> PerformReadyCheck();

        protected abstract void UpdateAllGeometry();

        #endregion

        #region Event Handlers

        /// <summary>
        /// called when the collection of monitors is changed after we initially instrumented everything,
        /// such as during reset monitors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Monitors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_started)
            {
                // not tracking
                return;
            }

            if (e.NewItems != null)
            {
                foreach (Monitor monitor in e.NewItems)
                {
                    AddMonitor(monitor);
                }
            }

            if (e.OldItems != null)
            {
                foreach (Monitor monitor in e.OldItems)
                {
                    RemoveMonitor(monitor);
                }
            }

            ScheduleGeometryChange();
        }

        /// <summary>
        /// react to a batch of changes to the viewport and monitor geometries
        /// </summary>
        protected void OnDelayedGeometryChange(object sender, EventArgs e)
        {
            if (!_monitorsValid)
            {
                // don't process events if monitors are not valid
                // we will schedule an update once they are
                _geometryChangeTimer?.Stop();
                return;
            }

            // recursion prevention flag
            _geometryChanging = true;
            try
            {
                _geometryChangeTimer?.Stop();
                if (Profile == null)
                {
                    // although we turn off the timer when we are removed from the profile, this call is still
                    // delivered late
                    return;
                }

                UpdateAllGeometry();
            }
            finally
            {
                _geometryChanging = false;
            }

            // notify anyone configuring or monitoring us
            GeometryChangeDelayed?.Invoke(this, EventArgs.Empty);

            // also send new status report
            InvalidateStatusReport();
        }

        protected void Profile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Profile.Name):
                    // WARNING: Path is changed before Name is set, so don't do this update on the change of "Path"
                    CurrentProfileName = string.IsNullOrWhiteSpace(Profile.Path) ? null : Profile.Name;
                    break;
                case nameof(Profile.Path):
                    if (!_started)
                    {
                        // not tracking
                        return;
                    }
                    InvalidateStatusReport();
                    break;
            }
        }

        /// <summary>
        /// called when the monitor dimensions are modified
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Raw_MonitorChanged(object sender, RawMonitorEventArgs e)
        {
            _ = sender;
            _ = e;
            UpdateGlobalOffset();
            ScheduleGeometryChange();
        }

        /// <summary>
        /// called when the viewport dimensions are modified
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Raw_ViewportChanged(object sender, RawViewportEventArgs e)
        {
            _ = sender;
            _ = e;
            ScheduleGeometryChange();
        }


        /// <summary>
        /// called when a configuration property on our model has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shadow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // configuration of our shadow object changed by UI, need to re-evaluate
            // configuration state
            ScheduleGeometryChange();
        }

        #endregion

        #region Overrides

        protected override void AttachToProfileOnMainThread()
        {
            CurrentProfileName = string.IsNullOrWhiteSpace(Profile.Path) ? null : Profile.Name;

            // NOTE: intentional crash if there is no Application.Current.Dispatcher
            _geometryChangeTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(100),
                DispatcherPriority.Normal,
                OnDelayedGeometryChange,
                Application.Current.Dispatcher)
            {
                IsEnabled = false
            };

            StartShadowing();

            // register for changes
            Profile.Monitors.CollectionChanged += Monitors_CollectionChanged;
            Profile.PropertyChanged += Profile_PropertyChanged;
        }

        protected override void DetachFromProfileOnMainThread(HeliosProfile oldProfile)
        {
            // stop updating shadow collections
            oldProfile.Monitors.CollectionChanged -= Monitors_CollectionChanged;
            oldProfile.PropertyChanged -= Profile_PropertyChanged;

            base.DetachFromProfileOnMainThread(oldProfile);

            // deallocate timer we allocate on Attach
            _geometryChangeTimer?.Stop();
            _geometryChangeTimer = null;

            ClearShadowObjects();
        }

        #endregion

        #region IExtendedDescription

        public abstract string Description { get; }
        public abstract string RemovalNarrative { get; }

        #endregion

        #region IResetMonitorsObserver

        public void NotifyResetMonitorsStarting()
        {
            if (!_started)
            {
                // not tracking
                return;
            }

            // not volatile, only main thread access
            _monitorsValid = false;
        }

        public void NotifyResetMonitorsComplete()
        {
            if (!_started)
            {
                // not tracking
                return;
            }

            // not volatile, only main thread access
            _monitorsValid = CheckMonitorsValid;
            if (!_monitorsValid)
            {
                return;
            }

            // rebuild the entire shadow tree because monitors may have switched 
            // identities due to their geometries being changed but the monitor
            // object stayed the same (problem caused by 69f90d13)
            ClearShadowObjects();
            CreateShadowObjects();
            ScheduleGeometryChange();
        }

        #endregion

        #region IShadowVisualParent

        public double Scale
        {
            get => _scale;
            set
            {
                double oldValue = _scale;
                if (Math.Abs(oldValue - value) < 0.001)
                {
                    return;
                }

                _scale = value;
                OnPropertyChanged("Scale", oldValue, value, true);
            }
        }

        public Vector GlobalOffset { get; private set; }

        public void AddViewport(ShadowVisual shadowViewport)
        {
            _viewports[shadowViewport.Visual] = shadowViewport;
            ViewportAdded?.Invoke(this, new ShadowViewportEventArgs(shadowViewport));

            // update viewport count on hosting monitor
            TMonitor monitor = _monitors[shadowViewport.Monitor];
            monitor.AddViewport();

            shadowViewport.ViewportChanged += Raw_ViewportChanged;

            // recalculate, delayed
            ScheduleGeometryChange();
        }

        public void RemoveViewport(ShadowVisual shadowViewport)
        {
            shadowViewport.ViewportChanged -= Raw_ViewportChanged;
            _viewports.Remove(shadowViewport.Visual);
            ViewportRemoved?.Invoke(this, new ShadowViewportEventArgs(shadowViewport));

            // update viewport count on hosting monitor
            TMonitor monitor = _monitors[shadowViewport.Monitor];
            monitor.RemoveViewport();

            // recalculate, delayed
            ScheduleGeometryChange();
        }

        #endregion

        #region IStatusReportNotify

        public void Subscribe(IStatusReportObserver observer)
        {
            _observers.Add(observer);
        }

        public void Unsubscribe(IStatusReportObserver observer)
        {
            _observers.Remove(observer);
        }

        public void InvalidateStatusReport()
        {
            if (_observers.Count < 1)
            {
                return;
            }

            List<StatusReportItem> newReport = CreateStatusReport();

            // publish report
            PublishStatusReport(newReport);
        }

        public void PublishStatusReport(IList<StatusReportItem> statusReport)
        {
            foreach (IStatusReportObserver observer in _observers)
            {
                observer.ReceiveStatusReport(Name,
                    Description,
                    statusReport);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// short name of profile currently being edited
        /// </summary>
        public string CurrentProfileName
        {
            get => _currentProfileName;
            set
            {
                if (_currentProfileName != null && _currentProfileName == value)
                {
                    return;
                }

                string oldValue = _currentProfileName;
                _currentProfileName = value;
                OnPropertyChanged("CurrentProfileName", oldValue, value, true);
            }
        }

        public bool CheckMonitorsValid => Profile != null && Profile.IsValidMonitorLayout && SameNumberOfMonitors;

        public IEnumerable<TMonitor> Monitors => _monitors.Values;

        public IEnumerable<ShadowVisual> Viewports => _viewports.Values;

        private bool SameNumberOfMonitors => Profile != null &&
                                             ConfigManager.DisplayManager.Displays.Count == Profile.Monitors.Count;

        #endregion

        public class GlobalOffsetEventArgs : EventArgs
        {
            public GlobalOffsetEventArgs(Vector globalOffset)
            {
                GlobalOffset = globalOffset;
            }

            #region Properties

            public Vector GlobalOffset { get; }

            #endregion
        }
    }
}