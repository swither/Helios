using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using GadrocsWorkshop.Helios.Interfaces.Capabilities;
using GadrocsWorkshop.Helios.Util;
using GadrocsWorkshop.Helios.Util.DCS;
using GadrocsWorkshop.Helios.Windows;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    internal class MonitorSetupViewModel : HeliosViewModel<MonitorSetup>, IStatusReportObserver
    {
        private readonly Dictionary<ShadowMonitor, MonitorViewModel> _monitors =
            new Dictionary<ShadowMonitor, MonitorViewModel>();

        private readonly Dictionary<ShadowVisual, ViewportViewModel> _viewports =
            new Dictionary<ShadowVisual, ViewportViewModel>();

        internal MonitorSetupViewModel(MonitorSetup data) : base(data)
        {
            Data.GeometryChangeDelayed += Data_GeometryChangeDelayed;
            Data.GlobalOffsetChanged += Data_GlobalOffsetChanged;
            Data.MonitorAdded += Data_MonitorAdded;
            Data.MonitorRemoved += Data_MonitorRemoved;
            Data.ViewportAdded += Data_ViewportAdded;
            Data.ViewportRemoved += Data_ViewportRemoved;

            Monitors = new ObservableCollection<MonitorViewModel>();
            Viewports = new ObservableCollection<ViewportViewModel>();

            foreach (ShadowMonitor monitor in Data.Monitors)
            {
                AddMonitor(monitor, Data.GlobalOffset);
            }

            foreach (ShadowVisual viewport in Data.Viewports)
            {
                AddViewport(viewport, Data.GlobalOffset);
            }

            UpdateAllGeometry();

            // register for status changes and get latest report
            Data.Subscribe(this);
            Data.InvalidateStatusReport();
        }

        private static void OnScaleChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MonitorSetupViewModel model = (MonitorSetupViewModel)d;
            foreach (MonitorViewModel monitor in model._monitors.Values)
            {
                monitor.Update(model.Scale);
            }

            foreach (ViewportViewModel viewport in model._viewports.Values)
            {
                viewport.Update(model.Scale);
            }
        }

        private void ProtectLastMonitor(List<MonitorViewModel> monitors, Action<MonitorViewModel, bool> setter)
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

        private void Data_ViewportRemoved(object sender, ShadowViewportEventArgs e)
        {
            if (_viewports.TryGetValue(e.Data, out ViewportViewModel model))
            {
                _viewports.Remove(e.Data);
                Viewports.Remove(model);
            }
        }

        private void Data_ViewportAdded(object sender, ShadowViewportEventArgs e)
        {
            IShadowVisualParent parent = (IShadowVisualParent) sender;
            AddViewport(e.Data, parent.GlobalOffset);
        }

        private void AddViewport(ShadowVisual viewport, Vector globalOffset)
        {
            ViewportViewModel model = new ViewportViewModel(viewport, globalOffset, Scale);
            _viewports[viewport] = model;
            Viewports.Add(model);
        }

        private void Data_MonitorRemoved(object sender, ShadowMonitorEventArgs e)
        {
            if (_monitors.TryGetValue(e.Data, out MonitorViewModel model))
            {
                _monitors.Remove(e.Data);
                Monitors.Remove(model);
                model.Data.PropertyChanged -= Shadow_PropertyChanged;
            }
        }

        private void Data_MonitorAdded(object sender, ShadowMonitorEventArgs e)
        {
            IShadowVisualParent parent = (IShadowVisualParent) sender;
            AddMonitor(e.Data, parent.GlobalOffset);
        }

        private void Shadow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // some settings about our shadow objects have changed
            UpdateAllGeometry();
        }

        private void AddMonitor(ShadowMonitor shadow, Vector globalOffset)
        {
            MonitorViewModel model = new MonitorViewModel(shadow, globalOffset, Scale);
            _monitors[shadow] = model;
            Monitors.Add(model);

            // register to changes to our settings (not the geometry of the observed visuals)
            model.Data.PropertyChanged += Shadow_PropertyChanged;
        }

        private void Data_GlobalOffsetChanged(object sender, MonitorSetup.GlobalOffsetEventArgs e)
        {
            foreach (MonitorViewModel monitor in _monitors.Values)
            {
                monitor.Update(e.GlobalOffset);
            }

            foreach (ViewportViewModel viewport in _viewports.Values)
            {
                viewport.Update(e.GlobalOffset);
            }
        }

        /// <summary>
        /// not automatically called via IDisposable, because of
        /// strong references in events
        /// </summary>
        public void Dispose()
        {
            Data.Unsubscribe(this);
            Data.GeometryChangeDelayed -= Data_GeometryChangeDelayed;
            Data.GlobalOffsetChanged -= Data_GlobalOffsetChanged;
            Data.MonitorAdded -= Data_MonitorAdded;
            Data.MonitorRemoved -= Data_MonitorRemoved;
            Data.ViewportAdded -= Data_ViewportAdded;
            Data.ViewportRemoved -= Data_ViewportRemoved;
        }

        private void Data_GeometryChangeDelayed(object sender, EventArgs e)
        {
            UpdateAllGeometry();
        }

        private void UpdateAllGeometry()
        {
            ClassifyMonitors(out List<MonitorViewModel> mainMonitors, out List<MonitorViewModel> uiMonitors);
            UpdateBounds();
            ProtectLastMonitor(mainMonitors, (m, value) => m.CanBeRemovedFromMain = value);
            ProtectLastMonitor(uiMonitors, (m, value) => m.CanBeRemovedFromUserInterface = value);
        }

        private void ClassifyMonitors(out List<MonitorViewModel> mainMonitors, out List<MonitorViewModel> uiMonitors)
        {
            List<MonitorViewModel> main = new List<MonitorViewModel>();
            List<MonitorViewModel> ui = new List<MonitorViewModel>();
            foreach (MonitorViewModel monitor in Monitors)
            {
                if (monitor.Data.Main)
                {
                    main.Add(monitor);
                }

                if (monitor.Data.UserInterface)
                {
                    ui.Add(monitor);
                }

                // REVISIT: we could have handled this in MonitorViewModel, but we already have to
                // update the geometry when viewports change
                if (monitor.HasContent)
                {
                    monitor.SetCanExclude(false,
                        "This monitor must be included in the area drawn by DCS because there is content on it.");
                }
                else
                {
                    monitor.SetCanExclude(true,
                        "This monitor can be removed from the area drawn by DCS because it is empty.");
                }
            }

            mainMonitors = main;
            uiMonitors = ui;
        }

        private void UpdateBounds()
        {
            Rect totalBounds = new Rect(0, 0, 1, 1);
            Rect bounds = new Rect(0, 0, 1, 1);
            Rect mainBounds = Rect.Empty;
            Rect uiBounds = Rect.Empty;

            foreach (MonitorViewModel monitor in Monitors)
            {
                totalBounds.Union(monitor.Rect);
                if (monitor.Data.Main)
                {
                    mainBounds.Union(monitor.Rect);
                }

                if (monitor.Data.UserInterface)
                {
                    uiBounds.Union(monitor.Rect);
                }

                if (!monitor.Data.Included)
                {
                    continue;
                }

                // this Rect is already translated to DCS coordinates and scaled for screen display
                bounds.Union(monitor.Rect);

                // convert monitor coordinates to DCS coordinates and always include 0,0 in the union
                // to calculate the resolution required by DCS
                Rect rawDcsCoordinates = monitor.RawRect;
                rawDcsCoordinates.Offset(Data.GlobalOffset);
            }

            ScaledResolutionWidth = bounds.Width;
            ScaledResolutionHeight = bounds.Height;
            ScaledTotalWidth = totalBounds.Width;
            ScaledTotalHeight = totalBounds.Height;
            ScaledMain = mainBounds;
            ScaledUserInterface = uiBounds;
        }

        public void ReceiveStatusReport(string name, string description, IList<StatusReportItem> statusReport)
        {
            _ = name;
            _ = description;

            // figure out our UI status
            StatusCodes newStatus = StatusCodes.Unknown;
            if (!Data.Profile.IsValidMonitorLayout)
            {
                newStatus = StatusCodes.ResetMonitorsRequired;
            }
            else if (string.IsNullOrWhiteSpace(Data.Profile.Path))
            {
                newStatus = StatusCodes.ProfileSaveRequired;
            }
            else if (!InstallationLocations.Singleton.Active.Any())
            {
                newStatus = StatusCodes.NoLocations;
            }
            else
            {
                // see if any items aren't "configuration up to date"
                bool outOfDate = statusReport.Any(item =>
                    !item.Flags.HasFlag(StatusReportItem.StatusFlags.ConfigurationUpToDate));
                newStatus = outOfDate ? StatusCodes.OutOfDate : StatusCodes.UpToDate;
            }

            Status = newStatus;
        }

        #region Properties

        public StatusCodes Status
        {
            get => (StatusCodes) GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(StatusCodes), typeof(MonitorSetupViewModel),
                new PropertyMetadata(StatusCodes.Unknown));

        public ObservableCollection<MonitorViewModel> Monitors
        {
            get => (ObservableCollection<MonitorViewModel>) GetValue(MonitorsProperty);
            set => SetValue(MonitorsProperty, value);
        }

        public static readonly DependencyProperty MonitorsProperty =
            DependencyProperty.Register("Monitors", typeof(ObservableCollection<MonitorViewModel>),
                typeof(MonitorSetupViewModel), new PropertyMetadata(null));

        public ObservableCollection<ViewportViewModel> Viewports
        {
            get => (ObservableCollection<ViewportViewModel>) GetValue(ViewportsProperty);
            set => SetValue(ViewportsProperty, value);
        }

        public static readonly DependencyProperty ViewportsProperty =
            DependencyProperty.Register("Viewports", typeof(ObservableCollection<ViewportViewModel>),
                typeof(MonitorSetupViewModel), new PropertyMetadata(null));

        public double ScaledTotalWidth
        {
            get => (double) GetValue(ScaledTotalWidthProperty);
            set => SetValue(ScaledTotalWidthProperty, value);
        }

        public static readonly DependencyProperty ScaledTotalWidthProperty =
            DependencyProperty.Register("ScaledTotalWidth", typeof(double), typeof(MonitorSetupViewModel),
                new PropertyMetadata(1.0));

        public double ScaledTotalHeight
        {
            get => (double) GetValue(ScaledTotalHeightProperty);
            set => SetValue(ScaledTotalHeightProperty, value);
        }

        public static readonly DependencyProperty ScaledTotalHeightProperty =
            DependencyProperty.Register("ScaledTotalHeight", typeof(double), typeof(MonitorSetupViewModel),
                new PropertyMetadata(1.0));

        public double ScaledResolutionWidth
        {
            get => (double) GetValue(ScaledResolutionWidthProperty);
            set => SetValue(ScaledResolutionWidthProperty, value);
        }

        public static readonly DependencyProperty ScaledResolutionWidthProperty =
            DependencyProperty.Register("ScaledResolutionWidth", typeof(double), typeof(MonitorSetupViewModel),
                new PropertyMetadata(1.0));

        public double ScaledResolutionHeight
        {
            get => (double) GetValue(ScaledResolutionHeightProperty);
            set => SetValue(ScaledResolutionHeightProperty, value);
        }

        public static readonly DependencyProperty ScaledResolutionHeightProperty =
            DependencyProperty.Register("ScaledResolutionHeight", typeof(double), typeof(MonitorSetupViewModel),
                new PropertyMetadata(1.0));

        public Rect ScaledMain
        {
            get => (Rect) GetValue(ScaledMainProperty);
            set => SetValue(ScaledMainProperty, value);
        }

        public static readonly DependencyProperty ScaledMainProperty =
            DependencyProperty.Register("ScaledMain", typeof(Rect), typeof(MonitorSetupViewModel),
                new PropertyMetadata(null));

        public Rect ScaledUserInterface
        {
            get => (Rect) GetValue(ScaledUserInterfaceProperty);
            set => SetValue(ScaledUserInterfaceProperty, value);
        }

        public static readonly DependencyProperty ScaledUserInterfaceProperty =
            DependencyProperty.Register("ScaledUserInterface", typeof(Rect), typeof(MonitorSetupViewModel),
                new PropertyMetadata(null));


        public double Scale
        {
            get => (double) GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(MonitorSetupViewModel),
                new PropertyMetadata(0.075, OnScaleChange));

        #endregion
    }
}