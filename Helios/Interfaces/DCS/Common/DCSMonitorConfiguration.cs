using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    // XXX make this its own HeliosInterface with its own editor
    public class DCSMonitorConfiguration: IReadyCheck
    {
        private DCSInterface _parent;

        /// <summary>
        /// for each viewport name, list of viewport extents that will be combined (will be multiple if view port spans monitors)
        /// </summary>
        private Dictionary<string, Rect> _viewports;
        private Rect _display;
        private List<Rect> _monitors;
        private List<IViewportExtent> _viewportInterfaces;
        private static readonly HashSet<string> _mainViewNames = new HashSet<string>
        {
            "center",
            "main"
        };

        public DCSMonitorConfiguration(DCSInterface dcsInterface)
        {
            _parent = dcsInterface;
            InventoryProfile();
        }

        private void InventoryProfile()
        {
            List<Rect> monitors = new List<Rect>();
            _display = new Rect(0d, 0d, 0d, 0d);
            _viewports = new Dictionary<string, Rect>();
            _viewportInterfaces = new List<IViewportExtent>();

            if (_parent.Profile == null)
            {
                throw new System.Exception("implementation error: monitor configuration object should not be instantiated on interface that is not in a profile");
            }

            // first find the display extent
            foreach (Monitor monitor in _parent.Profile.Monitors)
            {
                Rect monitorExtent = VisualToRect(monitor);
                monitors.Add(monitorExtent);
                _display.Union(monitorExtent);
            }

            // now recursively search for all viewports
            foreach (Monitor monitor in _parent.Profile.Monitors)
            {
                InventoryVisual(monitor, monitor);
            }

            // finally fix up monitors themselves to be in DCS coordinates
            // WARNING: these are structs, which is why we don't edit them in place
            _monitors = new List<Rect>();
            foreach (Rect monitorRect in monitors)
            {
                monitorRect.Offset(-_display.Left, -_display.Top);
                _monitors.Add(monitorRect);
            }
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
                if (_viewports.TryGetValue(viewport.ViewportName, out Rect viewRect))
                {
                    viewRect.Union(extent);
                }
                else
                {
                    _viewports.Add(viewport.ViewportName, extent);
                }
            }
            foreach (HeliosVisual child in visual.Children)
            {
                InventoryVisual(monitor, child);
            }
        }

        private static Rect VisualToRect(HeliosVisual visual)
        {
            return new Rect(new Point(visual.Left, visual.Top), new Point(visual.Left + visual.Width, visual.Top + visual.Height));
        }

        internal void GenerateMonitorSetup(string savedGamesName)
        {
            // update information
            InventoryProfile();

            string shortName = System.IO.Path.GetFileNameWithoutExtension(_parent.Profile.Path).Replace(" ", "");
            List<string> lines = new List<string>();

            // NOTE: why do we need to run this string through a local function?  does this create a ref somehow or prevent string interning?
            lines.Add("_  = function(p) return p; end;");
            lines.Add($"name = _('H_{shortName}')");
            lines.Add($"description = 'Generated from {shortName} Helios profile'");

            // identify main view
            Rect? main = null;
            foreach (KeyValuePair<string, Rect> viewPort in _viewports)
            {
                if (_mainViewNames.Contains(viewPort.Key.ToLower()))
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

            // generate main view if we don't have one
            if (!main.HasValue)
            {
                foreach (Rect monitor in _monitors)
                {
                    bool hasViewports = false;
                    foreach (Rect viewport in _viewports.Values)
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
                ConfigManager.LogManager.LogError("cannot generate monitor config since no main viewport was found");
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
            if (_viewports.ContainsKey("UI"))
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
            string monitorSetupsDirectory = System.IO.Path.Combine(Util.KnownFolders.SavedGames, savedGamesName, "Config", "MonitorSetup");
            System.IO.Directory.CreateDirectory(monitorSetupsDirectory);
            string monitorSetupPath = System.IO.Path.Combine(monitorSetupsDirectory, $"{shortName}.lua");
            System.IO.File.WriteAllText(monitorSetupPath, string.Join("\n", lines));

            // XXX optionally allow specify a shared monitor config and merging into it
        }

        public IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            // XXX check if monitor config exists and is selected
            InventoryProfile();
            IEnumerable<IViewportProvider> providers = _parent.Profile.Interfaces.OfType<IViewportProvider>();
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
    }
}
