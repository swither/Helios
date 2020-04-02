using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Xml;
using GadrocsWorkshop.Helios.ComponentModel;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    [HeliosInterface("Patching.DCS.MonitorSetup", "DCS Monitor Setup", typeof(MonitorSetupEditor), Factory = typeof(UniqueHeliosInterfaceFactory))]
    public partial class MonitorSetup: HeliosInterface, IReadyCheck, IStatusReportNotify, IResetMonitorsObserver, IShadowVisualParent
    {
        /// <summary>
        /// fired when the bounds of some view model items may have changed
        /// </summary>
        public event EventHandler GeometryChange;

        /// <summary>
        /// for each viewport name, list of viewport extents that will be combined (will be multiple if view port spans monitors)
        /// </summary>
        private Dictionary<string, Rect> _viewports1; // XXX remove
        private Rect _display; // XXX remove
        private List<Rect> _monitors1; // XXX remove
        private List<IViewportExtent> _viewportInterfaces; // XXX remove
        private static readonly HashSet<string> _mainViewNames = new HashSet<string>(System.StringComparer.CurrentCultureIgnoreCase)
        {
            "center",
            "main"
        };
        private HashSet<IStatusReportObserver> _observers = new HashSet<IStatusReportObserver>();
        internal const string SETTINGS_GROUP = "DCSMonitorSetup";
        
        /// <summary>
        /// inventory of our profile
        /// </summary>
        private Dictionary<string, ShadowMonitor> _monitors = new Dictionary<string, ShadowMonitor>();

        /// <summary>
        /// inventory of our profile
        /// </summary>
        private Dictionary<HeliosVisual, ShadowVisual> _viewports = new Dictionary<HeliosVisual, ShadowVisual>();

        /// <summary>
        /// ViewModels published for our interface
        /// </summary>
        public ObservableCollection<MonitorViewModel> Monitors { get; private set; }

        /// <summary>
        /// ViewModels published for our interface
        /// </summary>
        public ObservableCollection<ViewportViewModel> Viewports { get; private set; }

        /// <summary>
        /// scale used for the viewmodels
        /// </summary>
        private double _scale = 0.1;
        public double Scale
        {
            get { return _scale; }
            set
            {
                double oldValue = _scale;
                if (oldValue == value)
                {
                    return;
                }
                _scale = value;
                OnPropertyChanged("Scale", oldValue, value, true);
            }
        }

        public Vector GlobalOffset { get; private set; }

        public MonitorSetup()
            : base("DCS Monitor Setup")
        {
            // don't do any work here since we don't have a profile yet and we may just be
            // test instantiated for add interface dialog
        }

        private void InstrumentProfile()
        {
            // recursively walk profile and track every visual
            Monitors = new ObservableCollection<MonitorViewModel>();
            Viewports = new ObservableCollection<ViewportViewModel>();
            foreach (Monitor monitor in Profile.Monitors)
            {
                AddMonitor(monitor);
            }
            UpdateGlobalOffset();
            AutoSelectMainView();
            AutoSelectUserInterfaceView();
            Profile.Monitors.CollectionChanged += Monitors_CollectionChanged;
        }

        private void Clear()
        {
            foreach (ShadowVisual shadow in _monitors.Values)
            {
                shadow.Dispose();
            }
            _monitors.Clear();
            _viewports.Clear();
            Monitors.Clear();
            Viewports.Clear();
        }

        private void AddMonitor(Monitor monitor)
        {
            ShadowMonitor shadow = new ShadowMonitor(this, monitor);
            _monitors[shadow.Key] = shadow;
            Monitors.Add(shadow.MonitorViewModel);
            ConfigManager.LogManager.LogDebug($"created new scaled monitor view {shadow.MonitorViewModel.GetHashCode()} for monitor setup UI at {shadow.MonitorViewModel.Rect}");
        }

        private void RemoveMonitor(Monitor monitor)
        {
            string key = ShadowMonitor.CreateKey(monitor);
            ShadowMonitor shadowMonitor = _monitors[key];
            MonitorViewModel monitorViewModel = shadowMonitor.MonitorViewModel;
            ConfigManager.LogManager.LogDebug($"removed scaled monitor view {monitorViewModel.GetHashCode()} for monitor setup UI at {monitorViewModel.Rect}");
            Monitors.Remove(monitorViewModel);
            _monitors.Remove(key);
            shadowMonitor.Dispose();
        }

        private void InventoryProfile()
        {
            List<Rect> monitors = new List<Rect>();
            _display = new Rect(0d, 0d, 0d, 0d);
            _viewports1 = new Dictionary<string, Rect>();
            _viewportInterfaces = new List<IViewportExtent>();

            // first find the display extent
            foreach (Monitor monitor in Profile.Monitors)
            {
                Rect monitorExtent = VisualToRect(monitor);
                monitors.Add(monitorExtent);
                _display.Union(monitorExtent);
            }

            // now recursively search for all viewports
            foreach (Monitor monitor in Profile.Monitors)
            {
                InventoryVisual(monitor, monitor);
            }

            // finally fix up monitors themselves to be in DCS coordinates
            // WARNING: these are structs, which is why we don't edit them in place
            _monitors1 = new List<Rect>();
            foreach (Rect monitorRect in monitors)
            {
                monitorRect.Offset(-_display.Left, -_display.Top);
                _monitors1.Add(monitorRect);
            }
        }

        /// <summary>
        /// find the offset between windows coordinates and the 0,0 coordinate system used by DCS
        /// </summary>
        private void UpdateGlobalOffset()
        {
            double globalX = - _monitors.Values.Select(m => m.Visual.Left).Min<double>();
            double globalY = - _monitors.Values.Select(m => m.Visual.Top).Min<double>();
            GlobalOffset = new Vector(globalX, globalY);
            ConfigManager.LogManager.LogDebug($"new top left corner offset to translate from windows coordinates to DCS coordinates is {GlobalOffset}");

            // push this value to all viewports and monitors
            foreach (ViewportViewModel view in Viewports)
            {
                view.Update(GlobalOffset);
            }
            foreach (MonitorViewModel monitor in Monitors)
            {
                monitor.Update(GlobalOffset);
            }
            GeometryChange?.Invoke(this, EventArgs.Empty);
        }

        private void AutoSelectMainView()
        {
            // XXX add support for explicit main and UI view viewports, which will require updates to the editor also
            if (_monitors.Values.Any(m => m.MonitorViewModel.Main))
            {
                // got at least one
                return;
            }

            // count viewports per monitor
            Dictionary<string, int> candidates = new Dictionary<string, int>();
            foreach (string key in _monitors.Keys)
            {
                candidates[key] = 0;
            }
            foreach (ShadowVisual viewport in _viewports.Values)
            {
                string key = ShadowMonitor.CreateKey(viewport.Monitor);
                candidates[key] = candidates[key] + 1;
            }

            // select any monitors with minimal viewports (might not be entirely empty)
            int min = candidates.Values.Min<int>();
            foreach (string key in candidates.Where(pair => pair.Value == min).Select(pair => pair.Key))
            {
                _monitors[key].MonitorViewModel.Main = true;
            }
        }

        private void AutoSelectUserInterfaceView()
        {
            // XXX add support for explicit main and UI view viewports, which will require updates to the editor also
            if (_monitors.Values.Any(m => m.MonitorViewModel.UserInterface))
            {
                // got at least one
                return;
            }

            // count viewports per monitor
            Dictionary<string, int> candidates = new Dictionary<string, int>();
            foreach (string key in _monitors.Keys)
            {
                candidates[key] = 0;
            }
            foreach (ShadowVisual viewport in _viewports.Values)
            {
                string key = ShadowMonitor.CreateKey(viewport.Monitor);
                candidates[key] = candidates[key] + 1;
            }

            // select left-most largest monitor with minimal viewports (might not be entirely empty)
            int minViewPorts = candidates.Values.Min<int>();
            List<MonitorViewModel> minPopulated = _monitors
                .Where(pair => candidates[pair.Key] == minViewPorts)
                .OrderBy(pair => pair.Value.Monitor.Left)
                .Select(pair => pair.Value.MonitorViewModel).ToList();
            double maxSize = minPopulated.Select(m => m.Rect.Width).Max<double>();
            MonitorViewModel ui = minPopulated.First(m => m.Rect.Width == maxSize);
            ui.UserInterface = true;
        }

        private void InventoryVisual(Monitor monitor, HeliosVisual visual)
        {
            if (visual is IViewportExtent viewport)
            {
                // save for later
                _viewportInterfaces.Add(viewport);

                // get absolute extent based on monitor
                Rect extent = VisualToRect(visual);
                extent.Offset(monitor.Left, monitor.Top);

                // clip to monitor
                Rect monitorExtent = VisualToRect(monitor);
                extent.Intersect(monitorExtent);

                // now translate to DCS coordinates, which have 0,0 in the top left corner of the display
                extent.Offset(-_display.Left, -_display.Top);

                // merge by name, in case viewport extends across monitors, so we will
                // have multiple objects
                if (_viewports1.TryGetValue(viewport.ViewportName, out Rect viewRect))
                {
                    viewRect.Union(extent);
                }
                else
                {
                    _viewports1.Add(viewport.ViewportName, extent);
                }
            }
            foreach (HeliosVisual child in visual.Children)
            {
                InventoryVisual(monitor, child);
            }
        }

        public static Rect VisualToRect(HeliosVisual visual)
        {
            return new Rect(visual.Left, visual.Top, visual.Width, visual.Height);
        }

        // XXX call this with the dcsvariant of all selection InstallLocations
        internal void GenerateMonitorSetup(IEnumerable<string> savedGamesNames)
        {
            if (!Profile.IsValidMonitorLayout)
            {
                // XXX throw bad state, UI should have been disabled
            }

            // update information
            InventoryProfile();

            string shortName = System.IO.Path.GetFileNameWithoutExtension(Profile.Path).Replace(" ", "");
            List<string> lines = new List<string>();

            // NOTE: why do we need to run this string through a local function?  does this create a ref somehow or prevent string interning?
            lines.Add("_  = function(p) return p; end;");
            lines.Add($"name = _('H_{shortName}')");
            lines.Add($"description = 'Generated from {shortName} Helios profile'");

            // identify main view
            Rect? main = null;
            foreach (KeyValuePair<string, Rect> viewPort in _viewports1)
            {
                if (_mainViewNames.Contains(viewPort.Key))
                {
                    // found main view
                    main = viewPort.Value;
                }
                else
                {
                    // emit as extra viewport
                    lines.Add($"{viewPort.Key} = {{ x = {viewPort.Value.Left}, y = {viewPort.Value.Top}, width = {viewPort.Value.Width}, height = {viewPort.Value.Height} }}");
                }
            }

            // XXX use this for autoconfig once and then let it be changed by configuration
            // generate main view if we don't have one
            if (!main.HasValue)
            {
                foreach (Rect monitor in _monitors1)
                {
                    bool hasViewports = false;
                    foreach (Rect viewport in _viewports1.Values)
                    {
                        if (monitor.IntersectsWith(viewport))
                        {
                            hasViewports = true;
                            break;
                        }
                    }
                    if (!hasViewports)
                    {
                        // use this monitor as main
                        main = monitor;
                        break;
                    }
                }
            }

            if (!main.HasValue)
            {
                // XXX this needs to pop up in UI
                ConfigManager.LogManager.LogError("cannot generate monitor setup since no main viewport was found");
                return;
            }

            lines.Add("Viewports = {");
            lines.Add("  Center = {");
            lines.Add($"    x = {main.Value.Left},");
            lines.Add($"    y = {main.Value.Top},");
            lines.Add($"    width = {main.Value.Width},");
            lines.Add($"    height = {main.Value.Height},");
            lines.Add($"    aspect = {main.Value.Width / main.Value.Height},");
            lines.Add("    dx = 0,");
            lines.Add("    dy = 0");
            lines.Add("  }");
            lines.Add("}");

            // check for explicit UI view
            string uiView;
            if (_viewports1.ContainsKey("UI"))
            {
                uiView = "UI";
            }
            else
            {
                // XXX by default, run ui on only one monitor from main view
                uiView = "Viewports.Center";
            }

            lines.Add($"UIMainView = {uiView}");
            lines.Add("GU_MAIN_VIEWPORT = Viewports.Center");

            foreach (string line in lines)
            {
                ConfigManager.LogManager.LogDebug(line);
            }

            // write monitor config using base name of _parent.Profile
            foreach (string savedGamesName in savedGamesNames)
            {
                string monitorSetupsDirectory = System.IO.Path.Combine(Util.KnownFolders.SavedGames, savedGamesName, "Config", "MonitorSetup");
                System.IO.Directory.CreateDirectory(monitorSetupsDirectory);
                string monitorSetupPath = System.IO.Path.Combine(monitorSetupsDirectory, $"{shortName}.lua");
                System.IO.File.WriteAllText(monitorSetupPath, string.Join("\n", lines));
            }
            // XXX optionally allow specify a shared monitor config and merging into it
        }

        internal void Configure(InstallationLocations locations, InstallationDialogs installationDialogs)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            if (!Profile.IsValidMonitorLayout)
            {
                yield return new StatusReportItem
                {
                    Status = "Monitor configuration in profile does not match this computer",
                    Recommendation = "Using Helios Profile Editor, perform 'Reset Monitors' function.",
                    Severity = StatusReportItem.SeverityCode.Error
                };
                yield break;
            }

            InventoryProfile();

            // XXX check if any locations

            // XXX calculate monitor config

            // XXX check if correct monitor config exists and is selected in DCS (latter should be a gentle message but error)

            // XXX check if any referenced viewports require patches to work
            IEnumerable<IViewportProvider> providers = Profile.Interfaces.OfType<IViewportProvider>();
            foreach (IViewportExtent viewport in _viewportInterfaces.Where(v => v.RequiresPatches))
            {
                bool found = false;
                foreach (IViewportProvider provider in providers)
                {
                    if (provider.IsViewportAvailable(viewport.ViewportName))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    yield return new StatusReportItem
                    {
                        Status = $"viewport '{viewport.ViewportName}' requires patches to be installed",
                        Recommendation = $"using Helios Profile Editor, add an Additional Viewports interface or configure the viewport extent not to require patches",
                        Severity = StatusReportItem.SeverityCode.Error
                    };
                }
            }

            yield break;
        }

        public override void ReadXml(XmlReader reader)
        {
            // nothing in the profile
        }

        public override void WriteXml(XmlWriter writer)
        {
            // nothing in the profile
        }

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
            IEnumerable<StatusReportItem> newReport = PerformReadyCheck();
            PublishStatusReport(newReport);
        }

        public void PublishStatusReport(IEnumerable<StatusReportItem> statusReport)
        {
            foreach (IStatusReportObserver observer in _observers)
            {
                observer.ReceiveStatusReport(statusReport);
            }
        }

        /// <summary>
        /// notification from profile or UI that the monitors defined in the profile have been changed
        /// </summary>
        public void NotifyResetMonitorsComplete()
        {
            // make sure we have valid settings
            CheckMonitorSettings();

            // check on view selectons
            AutoSelectMainView();
            AutoSelectUserInterfaceView();

            // the monitors have already been updated by property change listeners and add/remove
            // monitor events, but now we are done and we can run the ready check again
            InvalidateStatusReport();
        }

        private void CheckMonitorSettings()
        {
            if (!Profile.IsValidMonitorLayout)
            {
                // hasn't been reset yet, these monitors aren't the ones will will use
                return;
            }

            // clean up settings for monitors that are not existing any more or are phantoms from unreset profiles
            ISettingsManager2 settingsManager2 = (ConfigManager.SettingsManager as ISettingsManager2);
            HashSet<string> displayKeys = new HashSet<string>();
            foreach (Monitor monitor in ConfigManager.DisplayManager.Displays) {
                displayKeys.UnionWith(ShadowMonitor.GetAllKeys(monitor));
            }
            List<string> stableKeys = settingsManager2.EnumerateSettingNames(SETTINGS_GROUP).ToList();
            foreach (string key in stableKeys)
            {
                if (!displayKeys.Contains(key)) 
                {
                    ConfigManager.LogManager.LogDebug($"removed setting '{key}' that does not refer to a real monitor");
                    settingsManager2.DeleteSetting(SETTINGS_GROUP, key);
                }
            }
        }

        protected override void OnProfileChanged(HeliosProfile oldProfile)
        {
            // WARNING: this method is called on the profile loading thread, so 
            // we must not directly create any dependency objects here
            base.OnProfileChanged(oldProfile);
            if (Profile != null)
            {
                // real initialization, not just a test instantiation

                // see if we need to start over if too much has changed
                CheckMonitorSettings();

                // read initial state and register for all changes,
                // must run on main thread because dependency objects are created
                if (Dispatcher == null)
                {
                    // this happens on add interface, but we are already on main thread
                    InstrumentProfile();
                }
                else
                {
                    // when loaded from a saved profile, this is called on the loader thread
                    Dispatcher.Invoke(InstrumentProfile);
                }
            }
        }

        private void Monitors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
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
            UpdateGlobalOffset();
            AutoSelectMainView();
            AutoSelectUserInterfaceView();
        }


        public void AddViewport(ShadowVisual viewport)
        {
            _viewports[viewport.Visual] = viewport;
            Viewports.Add(viewport.ViewportViewModel);
        }

        public void RemoveViewport(ShadowVisual viewport)
        {
            Viewports.Remove(viewport.ViewportViewModel);
            _viewports.Remove(viewport.Visual);
        }

        public void ChangeViewport(ShadowVisual viewport)
        {
            GeometryChange?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeMonitor(ShadowMonitor shadowMonitor)
        {
            UpdateGlobalOffset();
            GeometryChange?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeMonitorKey(ShadowMonitor shadowMonitor, string oldKey, string newKey)
        {
            _monitors.Remove(oldKey);
            _monitors[newKey] = shadowMonitor;
        }
    }
}
