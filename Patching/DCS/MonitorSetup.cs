using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Xml;
using GadrocsWorkshop.Helios.ComponentModel;

// XXX missing feature: allow specifying a shared monitor config and merging into it
// XXX missing feature: support explicit view ports for MAIN and UI

namespace GadrocsWorkshop.Helios.Patching.DCS
{

    [HeliosInterface("Patching.DCS.MonitorSetup", "DCS Monitor Setup", typeof(MonitorSetupEditor), Factory = typeof(UniqueHeliosInterfaceFactory))]
    public partial class MonitorSetup: HeliosInterface, IReadyCheck, IStatusReportNotify, IResetMonitorsObserver, IShadowVisualParent, IInstallation
    {
        /// <summary>
        /// fired when the bounds of some view model items may have changed
        /// </summary>
        public event EventHandler GeometryChange;

        /// <summary>
        /// magic names of viewports that indicate that a viewport should be part of the main view rectangle (XXX unimplemented)
        /// </summary>
        private static readonly HashSet<string> _mainViewNames = new HashSet<string>(System.StringComparer.CurrentCultureIgnoreCase)
        {
            "center",
            "main"
        };
        internal const string SETTINGS_GROUP = "DCSMonitorSetup";

        private HashSet<IStatusReportObserver> _observers = new HashSet<IStatusReportObserver>();

        /// <summary>
        /// live inventory of our profile
        /// </summary>
        private Dictionary<string, ShadowMonitor> _monitors = new Dictionary<string, ShadowMonitor>();

        /// <summary>
        /// live inventory of our profile
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

        /// <summary>
        /// the entire text of the monitor setup lua file
        /// </summary>
        private string _monitorSetup;

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
            UpdateAllGeometry();

            // register for changes
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

            // now that we can find the monitor in our index, we can safely add viewports
            // and other children
            shadow.Instrument();

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
        }

        /// <summary>
        /// if we somehow end up with no main view, select one
        /// 
        /// this also happens when we start fresh
        /// </summary>
        private void AutoSelectMainView()
        {
            // XXX add support for explicit main and UI view viewports, which will require updates to the editor also
            if (_monitors.Values.Any(m => m.MonitorViewModel.Main))
            {
                // got at least one
                return;
            }
    
            // select any monitors with minimal viewports (might not be entirely empty)
            int min = _monitors.Values
                .Select<ShadowMonitor, int>(m => m.ViewportCount)
                .Min<int>();
            foreach (ShadowMonitor monitor in _monitors.Values.Where(m => m.ViewportCount == min))
            {
                monitor.MonitorViewModel.Main = true;
            }
        }

        /// <summary>
        /// if we somehow end up with no UI view, select one
        /// 
        /// this also happens when we start fresh
        /// </summary>
        private void AutoSelectUserInterfaceView()
        {
            // XXX add support for explicit main and UI view viewports, which will require updates to the editor also
            if (_monitors.Values.Any(m => m.MonitorViewModel.UserInterface))
            {
                // got at least one
                return;
            }

            // select any monitors with minimal viewports (might not be entirely empty)
            int minViewports = _monitors.Values
                .Select<ShadowMonitor, int>(m => m.ViewportCount)
                .Min<int>();
            IOrderedEnumerable<ShadowMonitor> sortedMinPopulated = _monitors.Values
                .Where(m => m.ViewportCount == minViewports)
                .OrderBy(m => m.Monitor.Left);
            double maxSize = sortedMinPopulated.Select(m => m.Monitor.Width).Max<double>();
            ShadowMonitor ui = sortedMinPopulated.First(m => m.Monitor.Width == maxSize);
            ui.MonitorViewModel.UserInterface = true;
        }

        public static Rect VisualToRect(HeliosVisual visual)
        {
            return new Rect(visual.Left, visual.Top, visual.Width, visual.Height);
        }

        /// <summary>
        /// generate the monitor setup file but do not write it out yet
        /// </summary>
        /// <returns></returns>
        private IEnumerable<StatusReportItem> UpdateMonitorSetup()
        {
            string shortName = GenerateShortName();
            List<string> lines = new List<string>();

            // NOTE: why do we need to run this string through a local function?  does this create a ref somehow or prevent string interning?
            lines.Add("_  = function(p) return p; end;");
            lines.Add($"name = _('H_{shortName}')");
            lines.Add($"description = 'Generated from {shortName} Helios profile'");

            // emit extra viewports
            foreach (ShadowVisual viewPort in _viewports.Values)
            {
                Rect rect = VisualToRect(viewPort.Visual);
                rect.Offset(viewPort.Monitor.Left, viewPort.Monitor.Top);
                rect.Offset(GlobalOffset);

                string code = $"{viewPort.Viewport.ViewportName} = {{ x = {rect.Left}, y = {rect.Top}, width = {rect.Width}, height = {rect.Height} }}";
                yield return new StatusReportItem
                {
                    Status = code,
                    Flags = StatusReportItem.StatusFlags.Verbose | StatusReportItem.StatusFlags.ConfigurationUpToDate
                };
                lines.Add(code);
            }

            // find main and ui view extents
            Rect mainView = Rect.Empty;
            Rect uiView = Rect.Empty;
            foreach (ShadowMonitor monitor in _monitors.Values)
            {
                Rect rect = VisualToRect(monitor.Monitor);
                if (monitor.MonitorViewModel.Main)
                {
                    mainView.Union(rect);
                }
                if (monitor.MonitorViewModel.UserInterface)
                {
                    uiView.Union(rect);
                }
            }
            mainView.Offset(GlobalOffset);
            uiView.Offset(GlobalOffset);

            // calling the MAIN viewport "Center" to match DCS' built-in monitor setups, even though it doesn't matter
            lines.Add("Viewports = {");
            lines.Add("  Center = {");
            lines.Add($"    x = {mainView.Left},");
            lines.Add($"    y = {mainView.Top},");
            lines.Add($"    width = {mainView.Width},");
            lines.Add($"    height = {mainView.Height},");
            lines.Add($"    aspect = {mainView.Width / mainView.Height},");
            lines.Add("    dx = 0,");
            lines.Add("    dy = 0");
            lines.Add("  }");
            lines.Add("}");
            yield return new StatusReportItem
            {
                Status = $"MAIN = {{ x = {mainView.Left}, y = {mainView.Top}, width = {mainView.Width}, height = {mainView.Height} }}",
                Flags = StatusReportItem.StatusFlags.Verbose | StatusReportItem.StatusFlags.ConfigurationUpToDate
            };

            // check for separate UI view
            yield return CreateUserInterfaceViewIfRequired(lines, mainView, uiView, out string uiViewName);

            // main set up required names for viewports (well-known to DCS)
            lines.Add($"UIMainView = {uiViewName}");
            lines.Add("GU_MAIN_VIEWPORT = Viewports.Center");

            foreach (string line in lines)
            {
                ConfigManager.LogManager.LogDebug(line);
            }

            _monitorSetup = string.Join("\n", lines);
        }

        /// <summary>
        /// create separate UI view if not identical to the main view
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="mainView"></param>
        /// <param name="uiView"></param>
        /// <param name="uiViewName"></param>
        /// <returns></returns>
        private StatusReportItem CreateUserInterfaceViewIfRequired(List<string> lines, Rect mainView, Rect uiView, out string uiViewName)
        {
            string comment;
            if (uiView != mainView)
            {
                uiViewName = "UI";
                string code = $"{uiViewName} = {{ x = {uiView.Left}, y = {uiView.Top}, width = {uiView.Width}, height = {uiView.Height} }}";
                comment = code;
                lines.Add(code);
            }
            else
            {
                uiViewName = "Viewports.Center";
                comment = "UI = MAIN";
            }
            return new StatusReportItem
            {
                Status = comment,
                Flags = StatusReportItem.StatusFlags.Verbose | StatusReportItem.StatusFlags.ConfigurationUpToDate
            };
        }

        /// <summary>
        /// attempt to write the monitor setup file to all configured Saved Games folders
        /// </summary>
        /// <param name="callbacks"></param>
        /// <returns></returns>
        public InstallationResult Install(IInstallationCallbacks callbacks)
        {
            try
            {
                if (!Profile.IsValidMonitorLayout)
                {
                    throw new System.Exception("UI should have disabled monitor setup writing without up to date monitors; implementation error");
                }
                // WARNING: have to read the enumeration or it won't run (yield return)
                List<StatusReportItem> results = new List<StatusReportItem>(EnumerateMonitorSetupFiles(InstallFile));
                foreach (StatusReportItem item in results)
                {
                    if (item.Severity >= StatusReportItem.SeverityCode.Error)
                    {
                        callbacks.Failure(
                            "Monitor setup file generation has failed", 
                            "Some files may have been generated before the failure and these files were not removed.",
                            results);
                        return InstallationResult.Fatal;
                    }
                    // REVISIT we should have simulated first to gather warnings and errors so we can show a danger prompt
                }
                callbacks.Success(
                    "Monitor setup generation successful", 
                    "Monitor setup files have been installed into DCS.",
                    results);
                return InstallationResult.Success;
            }
            catch (Exception ex)
            {
                ConfigManager.LogManager.LogError("failed to install monitor setup", ex);
                callbacks.Failure("Failed to install monitor setup", ex.Message, new List<StatusReportItem>());
                return InstallationResult.Fatal;
            }
        }

        private StatusReportItem InstallFile(InstallationLocation location, string name, string directoryPath, string filePath)
        {
            System.IO.Directory.CreateDirectory(directoryPath);
            System.IO.File.WriteAllText(filePath, _monitorSetup);
            return new StatusReportItem
            {
                Status = $"generated monitor setup file '{name}' in {GenerateAnonymousPath(location.SavedGamesName)}",
                Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
            };
        }

        private static string GenerateSavedGamesPath(string savedGamesName)
        {
            return System.IO.Path.Combine(Util.KnownFolders.SavedGames, savedGamesName, "Config", "MonitorSetup");
        }

        private object GenerateAnonymousPath(string savedGamesName)
        {
            string savedGamesTranslated = System.IO.Path.GetFileName(Util.KnownFolders.SavedGames);
            return System.IO.Path.Combine(savedGamesTranslated, savedGamesName, "Config", "MonitorSetup");
        }
    
        private string GenerateFileName()
        {
            return $"{GenerateShortName()}.lua";
        }

        private string GenerateShortName()
        {
            return System.IO.Path.GetFileNameWithoutExtension(Profile.Path).Replace(" ", "");
        }

        private delegate StatusReportItem ProcessMonitorSetupFile(InstallationLocation location, string name, string directoryPath, string filePath);

        private IEnumerable<StatusReportItem> EnumerateMonitorSetupFiles(ProcessMonitorSetupFile handler)
        {
            string fileName = GenerateFileName();
            HashSet<string> done = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (InstallationLocation location in InstallationLocations.Singleton.Items.Where(l => l.IsEnabled))
            {
                if (done.Contains(location.SavedGamesName))
                {
                    // could have defaulted or configured the same variant twice
                    continue;
                }
                string monitorSetupsDirectory = GenerateSavedGamesPath(location.SavedGamesName);
                string monitorSetupPath = System.IO.Path.Combine(monitorSetupsDirectory, fileName);
                yield return handler(location, fileName, monitorSetupsDirectory, monitorSetupPath);
            }
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

            // check if DCS install folders are configured
            InstallationLocations locations = InstallationLocations.Singleton;
            if (locations.Items.Where(l => l.IsEnabled).Count() < 1)
            {
                yield return new StatusReportItem
                {
                    Status = $"No DCS installation locations are configured for monitor setup",
                    Recommendation = $"Using Helios Profile Editor, configure any DCS installation directories you use",
                    Severity = StatusReportItem.SeverityCode.Error
                };
            }

            // check if any referenced viewports require patches to work
            IEnumerable<IViewportProvider> providers = Profile.Interfaces.OfType<IViewportProvider>();
            foreach (IViewportExtent viewport in _viewports.Values
                .Select(shadow => shadow.Viewport)
                .Where(v => v.RequiresPatches))
            {
                bool found = false;
                foreach (IViewportProvider provider in providers)
                {
                    if (provider.IsViewportAvailable(viewport.ViewportName))
                    {
                        yield return new StatusReportItem
                        {
                            Status = $"viewport '{viewport.ViewportName}' is provided by '{(provider as HeliosInterface).Name}'",
                            Flags = StatusReportItem.StatusFlags.Verbose | StatusReportItem.StatusFlags.ConfigurationUpToDate
                        };
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

            // calculate monitor config
            foreach (StatusReportItem item in UpdateMonitorSetup())
            {
                yield return item;
            }

            // see if it is up to date in all locations
            bool updated = true;
            bool hasFile = false;
            foreach (StatusReportItem item in EnumerateMonitorSetupFiles(CheckSetupFile))
            {
                if (!item.Flags.HasFlag(StatusReportItem.StatusFlags.ConfigurationUpToDate))
                {
                    updated = false;
                }
                hasFile = true;
                yield return item;
            }

            // XXX check if monitor setup selected in DCS (should be a gentle message but error)
            // XXX check if correct monitor resolution selected in DCS (should be a gentle message but error)
            yield return new StatusReportItem
            {
                Status = "This version of Helios does not configure the selected Resolution in DCS directly",
                Recommendation = $"Using DCS, please set 'Resolution' in the graphical Options to the value displayed in {Name}",
                Severity = StatusReportItem.SeverityCode.Info,
                Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
            };

            // don't tell the user to do this yet if the file isn't done
            if (hasFile && updated)
            {
                yield return new StatusReportItem
                {
                    Status = "This version of Helios does not configure the selected monitor setup in DCS directly",
                    Recommendation = $"Using DCS, please set 'Monitors' in the graphical Options to '{GenerateShortName()}'",
                    Severity = StatusReportItem.SeverityCode.Info,
                    Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                };
            }

            yield break;
        }

        private StatusReportItem CheckSetupFile(InstallationLocation location, string name, string directoryPath, string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return new StatusReportItem
                {
                    Status = $"{GenerateAnonymousPath(location.SavedGamesName)} does not contain the monitor setup file '{name}'",
                    Recommendation = $"Using Helios Profile Editor, generate the file or disable {Name}",
                    Severity = StatusReportItem.SeverityCode.Warning
                };
            }
            string contents = System.IO.File.ReadAllText(filePath);
            if (contents != _monitorSetup)
            {
                return new StatusReportItem
                {
                    Status = $"monitor setup file '{name}' in {GenerateAnonymousPath(location.SavedGamesName)} does not match configuration",
                    Recommendation = $"Using Helios Profile Editor, regenerate the file or disable {this.Name}",
                    Severity = StatusReportItem.SeverityCode.Warning
                };
            }
            return new StatusReportItem
            {
                Status = $"monitor setup file '{name}' in {GenerateAnonymousPath(location.SavedGamesName)} is up to date",
                Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
            };
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
            OnMonitorCollectionChange();
        }

        /// <summary>
        /// notify anyone configuring us that there was some change to the viewport and monitor geometries
        /// </summary>
        private void OnGeometryChange()
        {
            UpdateAllGeometry();

            // notify anyone configuring or monitoring us
            GeometryChange?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// check monitor settings and clean up any abandoned ones
        /// 
        /// this is necessary because when we run with a profile that has not been reset,
        /// we end up with settings for monitors that don't actually exist.  Also, actual
        /// changes to our windows monitors will abandon settings for monitors that have 
        /// changed locations or have been removed.
        /// </summary>
        /// <returns>true if any settings were deleted</returns>
        private bool CheckMonitorSettings()
        {
            if (!Profile.IsValidMonitorLayout)
            {
                // hasn't been reset yet, these monitors aren't the ones will will use
                return true;
            }

            // clean up settings for monitors that are not existing any more or are phantoms from unreset profiles
            bool changesMade = false;

            // get the list of real monitors
            HashSet<string> displayKeys = new HashSet<string>();
            foreach (Monitor monitor in ConfigManager.DisplayManager.Displays) {
                displayKeys.UnionWith(ShadowMonitor.GetAllKeys(monitor));
            }

            // get the list of configured monitors in the settings file
            ISettingsManager2 settingsManager2 = (ConfigManager.SettingsManager as ISettingsManager2);
            List<string> stableKeys = settingsManager2.EnumerateSettingNames(SETTINGS_GROUP).ToList();

            // compare
            foreach (string key in stableKeys)
            {
                if (!displayKeys.Contains(key)) 
                {
                    ConfigManager.LogManager.LogDebug($"removed setting '{key}' that does not refer to a real monitor");
                    settingsManager2.DeleteSetting(SETTINGS_GROUP, key);
                    changesMade = true;
                }
            }

            return changesMade;
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
            OnMonitorCollectionChange();
        }

        /// <summary>
        /// handle any changes that may have added or removed monitors
        /// </summary>
        private void OnMonitorCollectionChange()
        {
            OnGeometryChange();
        }

        private void UpdateAllGeometry()
        {
            CheckMonitorSettings();
            AutoSelectMainView();
            AutoSelectUserInterfaceView();
            UpdateGlobalOffset();
        }

        public void AddViewport(ShadowVisual viewport)
        {
            _viewports[viewport.Visual] = viewport;
            Viewports.Add(viewport.ViewportViewModel);

            // update viewport count on hosting monitor
            string monitorKey = ShadowMonitor.CreateKey(viewport.Monitor);
            ShadowMonitor monitor = _monitors[monitorKey];
            if (monitor.AddViewport())
            {
                // need to recalculate everything: monitor now has viewports
                OnGeometryChange();
            }
            else
            {
                // just let the UI know something is changed, so we can regenerate status etc.
                GeometryChange?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RemoveViewport(ShadowVisual viewport)
        {
            Viewports.Remove(viewport.ViewportViewModel);
            _viewports.Remove(viewport.Visual);

            // update viewport count on hosting monitor
            string monitorKey = ShadowMonitor.CreateKey(viewport.Monitor);
            ShadowMonitor monitor = _monitors[monitorKey];
            if (monitor.RemoveViewport())
            {
                // need to recalculate everything: monitor no longer has viewports
                OnGeometryChange();
            }
            else
            {
                // just let the UI know something is changed, so we can regenerate status etc.
                GeometryChange?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ChangeViewport(ShadowVisual viewport)
        {
            GeometryChange?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeMonitor(ShadowMonitor shadowMonitor)
        {
            UpdateGlobalOffset();
            OnGeometryChange();
        }

        public void ChangeMonitorKey(ShadowMonitor shadowMonitor, string oldKey, string newKey)
        {
            _monitors.Remove(oldKey);
            _monitors[newKey] = shadowMonitor;
        }
    }
}
