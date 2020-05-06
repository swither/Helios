using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using GadrocsWorkshop.Helios.Interfaces.Capabilities;
using GadrocsWorkshop.Helios.Util;
using GadrocsWorkshop.Helios.Util.DCS;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    internal class MonitorSetupGenerator : DCSConfiguration
    {
        #region Nested

        private delegate StatusReportItem ProcessMonitorSetupFile(InstallationLocation location, string name,
            string directoryPath, string filePath);

        #endregion

        #region Private

        private readonly MonitorSetup _parent;

        /// <summary>
        /// the entire text of the monitor setup lua file
        /// </summary>
        private string _monitorSetup;

        #endregion

        public MonitorSetupGenerator(MonitorSetup parent)
        {
            _parent = parent;
            SubscribeToLocationChanges();
            _parent.Profile.PropertyChanged += Profile_PropertyChanged;
        }

        private void Profile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Path":
                    Update();
                    break;
            }
        }

        protected override void Update()
        {
            _parent.InvalidateStatusReport();
        }

        private const StatusReportItem.StatusFlags Informational = StatusReportItem.StatusFlags.Verbose | StatusReportItem.StatusFlags.ConfigurationUpToDate;

        /// <summary>
        /// generate the monitor setup file but do not write it out yet
        /// </summary>
        /// <returns></returns>
        public IEnumerable<StatusReportItem> UpdateMonitorSetup()
        {
            string shortName = GenerateShortName();
            List<string> lines = new List<string>
            {
                // NOTE: why do we need to run this string through a local function?  does this create a ref somehow or prevent string interning?
                "_  = function(p) return p; end;",
                $"name = _('H_{shortName}')",
                $"description = 'Generated from {shortName} Helios profile'"
            };


            // emit extra viewports in canonical order, so we can compare files later
            foreach (ShadowVisual viewPort in _parent.Viewports.OrderBy(v => v.Viewport.ViewportName))
            {
                Rect rect = MonitorSetup.VisualToRect(viewPort.Visual);
                rect.Offset(viewPort.Monitor.Left, viewPort.Monitor.Top);
                rect.Offset(_parent.GlobalOffset);

                string code =
                    $"{viewPort.Viewport.ViewportName} = {{ x = {rect.Left}, y = {rect.Top}, width = {rect.Width}, height = {rect.Height} }}";
                yield return new StatusReportItem
                {
                    Status = code,
                    Flags = Informational
                };
                lines.Add(code);
            }

            // find main and ui view extents
            Rect mainView = Rect.Empty;
            Rect uiView = Rect.Empty;
            foreach (ShadowMonitor monitor in _parent.Monitors)
            {
                string monitorDescription =
                    $"Monitor at Windows coordinates ({monitor.Monitor.Left},{monitor.Monitor.Top}) of size {monitor.Monitor.Width}x{monitor.Monitor.Height}";
                if (!monitor.Included)
                {
                    yield return new StatusReportItem
                    {
                        Status = $"{monitorDescription} is not included in monitor setup",
                        Flags = Informational
                    };
                    continue;
                }

                Rect rect = MonitorSetup.VisualToRect(monitor.Monitor);
                if (monitor.Main)
                {
                    mainView.Union(rect);
                    yield return new StatusReportItem
                    {
                        Status = $"{monitorDescription} is used for main view",
                        Flags = Informational
                    };
                }

                if (monitor.UserInterface)
                {
                    uiView.Union(rect);
                    yield return new StatusReportItem
                    {
                        Status = $"{monitorDescription} is used to display the DCS user interface",
                        Flags = Informational
                    };
                }

                if (monitor.ViewportCount > 0)
                {
                    string plural = (monitor.ViewportCount > 1) ? "s" : "";
                    yield return new StatusReportItem
                    {
                        Status = $"{monitorDescription} has {monitor.ViewportCount} viewport{plural}",
                        Flags = Informational
                    };
                }
            }

            mainView.Offset(_parent.GlobalOffset);
            uiView.Offset(_parent.GlobalOffset);

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
                Status =
                    $"MAIN = {{ x = {mainView.Left}, y = {mainView.Top}, width = {mainView.Width}, height = {mainView.Height} }}",
                Flags = Informational
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

        public object GenerateAnonymousPath(string savedGamesName)
        {
            string savedGamesTranslated = Path.GetFileName(KnownFolders.SavedGames);
            return Path.Combine(savedGamesTranslated, savedGamesName, "Config", "MonitorSetup");
        }

        public string GenerateFileName() => $"{GenerateShortName()}.lua";

        public string GenerateShortName() => Path.GetFileNameWithoutExtension(_parent.Profile.Path).Replace(" ", "");

        private string GenerateSavedGamesPath(string savedGamesName) =>
            Path.Combine(KnownFolders.SavedGames, savedGamesName, "Config", "MonitorSetup");

        private IEnumerable<StatusReportItem> EnumerateMonitorSetupFiles(ProcessMonitorSetupFile handler)
        {
            string fileName = GenerateFileName();
            HashSet<string> done = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (InstallationLocation location in InstallationLocations.Singleton.Active)
            {
                if (done.Contains(location.SavedGamesName))
                {
                    // could have defaulted or configured the same variant twice
                    continue;
                }

                string monitorSetupsDirectory = GenerateSavedGamesPath(location.SavedGamesName);
                string monitorSetupPath = Path.Combine(monitorSetupsDirectory, fileName);
                yield return handler(location, fileName, monitorSetupsDirectory, monitorSetupPath);
            }
        }

        public StatusReportItem InstallFile(InstallationLocation location, string name, string directoryPath,
            string filePath)
        {
            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(filePath, _monitorSetup);
            return new StatusReportItem
            {
                Status = $"generated monitor setup file '{name}' in {GenerateAnonymousPath(location.SavedGamesName)}",
                Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
            };
        }

        public StatusReportItem CheckSetupFile(InstallationLocation location, string name,
            string _, string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new StatusReportItem
                {
                    Status =
                        $"{GenerateAnonymousPath(location.SavedGamesName)} does not contain the monitor setup file '{name}'",
                    Recommendation = $"Configure {_parent.Name}",
                    Link = StatusReportItem.ProfileEditor,
                    Severity = StatusReportItem.SeverityCode.Warning
                };
            }

            string contents = File.ReadAllText(filePath);
            if (contents != _monitorSetup)
            {
                return new StatusReportItem
                {
                    Status =
                        $"monitor setup file '{name}' in {GenerateAnonymousPath(location.SavedGamesName)} does not match configuration",
                    Recommendation = $"Configure {_parent.Name}",
                    Link = StatusReportItem.ProfileEditor,
                    Severity = StatusReportItem.SeverityCode.Warning
                };
            }

            return new StatusReportItem
            {
                Status =
                    $"monitor setup file '{name}' in {GenerateAnonymousPath(location.SavedGamesName)} is up to date",
                Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
            };
        }

        /// <summary>
        /// create separate UI view if not identical to the main view
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="mainView"></param>
        /// <param name="uiView"></param>
        /// <param name="uiViewName"></param>
        /// <returns></returns>
        public StatusReportItem CreateUserInterfaceViewIfRequired(List<string> lines, Rect mainView, Rect uiView,
            out string uiViewName)
        {
            string comment;
            if (uiView != mainView)
            {
                uiViewName = "UI";
                string code =
                    $"{uiViewName} = {{ x = {uiView.Left}, y = {uiView.Top}, width = {uiView.Width}, height = {uiView.Height} }}";
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
        public override InstallationResult Install(IInstallationCallbacks callbacks)
        {
            try
            {
                if (!_parent.Profile.IsValidMonitorLayout)
                {
                    throw new Exception(
                        "UI should have disabled monitor setup writing without up to date monitors; implementation error");
                }

                if (string.IsNullOrWhiteSpace(_parent.Profile.Path))
                {
                    throw new Exception(
                        "UI should have disabled monitor setup without a profile name; implementation error");
                }

                // WARNING: have to read the enumeration or it won't run (yield return)
                List<StatusReportItem> results = new List<StatusReportItem>(
                    EnumerateMonitorSetupFiles(InstallFile));
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
                _parent.InvalidateStatusReport();
                return InstallationResult.Success;
            }
            catch (Exception ex)
            {
                ConfigManager.LogManager.LogError("failed to install monitor setup", ex);
                callbacks.Failure("Failed to install monitor setup", ex.Message, new List<StatusReportItem>());
                _parent.InvalidateStatusReport();
                return InstallationResult.Fatal;
            }
        }

        public override IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            if (!_parent.Profile.IsValidMonitorLayout)
            {
                yield return new StatusReportItem
                {
                    Status = "Monitor configuration in profile does not match this computer",
                    Recommendation = "Perform 'Reset Monitors' function from the 'Profile' menu",
                    Link = StatusReportItem.ProfileEditor,
                    Severity = StatusReportItem.SeverityCode.Error
                };
                yield break;
            }

            if (string.IsNullOrWhiteSpace(_parent.Profile.Path))
            {
                yield return new StatusReportItem
                {
                    Status = "You must save the profile before you can use Monitor Setup, because it uses the profile name as the name of the monitor setup",
                    Recommendation = "Save the profile at least once before configuring Monitor Setup",
                    Link = StatusReportItem.ProfileEditor,
                    Severity = StatusReportItem.SeverityCode.Error
                };
                yield break;
            }

            // check if DCS install folders are configured
            InstallationLocations locations = InstallationLocations.Singleton;
            if (!locations.Active.Any())
            {
                yield return new StatusReportItem
                {
                    Status = "No DCS installation locations are configured for monitor setup",
                    Recommendation = "Configure any DCS installation location you use",
                    Link = StatusReportItem.ProfileEditor,
                    Severity = StatusReportItem.SeverityCode.Error
                };
            }

            // check if any referenced viewports require patches to work
            IEnumerable<IViewportProvider> viewportProviders =
                _parent.Profile.Interfaces.OfType<IViewportProvider>().ToList();
            foreach (IViewportExtent viewport in _parent.Viewports
                .Select(shadow => shadow.Viewport)
                .Where(v => v.RequiresPatches))
            {
                bool found = false;
                foreach (IViewportProvider provider in viewportProviders)
                {
                    if (provider.IsViewportAvailable(viewport.ViewportName))
                    {
                        yield return new StatusReportItem
                        {
                            Status =
                                $"viewport '{viewport.ViewportName}' is provided by '{((HeliosInterface) provider).Name}'",
                            Flags = StatusReportItem.StatusFlags.Verbose |
                                    StatusReportItem.StatusFlags.ConfigurationUpToDate
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
                        Recommendation =
                            "Add an Additional Viewports interface or configure the viewport extent not to require patches",
                        Link = StatusReportItem.ProfileEditor,
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
                Status = "This version of Helios does not select the Resolution in DCS directly",
                Recommendation =
                    $"Using DCS, please set 'Resolution' in the 'System' options to at least {_parent.Resolution.X}x{_parent.Resolution.Y}",
                Severity = StatusReportItem.SeverityCode.Info,
                Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
            };

            // don't tell the user to do this yet if the file isn't done
            if (hasFile && updated)
            {
                yield return new StatusReportItem
                {
                    Status = "This version of Helios does not select the monitor setup in DCS directly",
                    Recommendation =
                        $"Using DCS, please set 'Monitors' in the 'System' options to 'H_{GenerateShortName()}'",
                    Severity = StatusReportItem.SeverityCode.Info,
                    Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                };
                yield return new StatusReportItem
                {
                    Status = "This version of Helios generates a separate monitor setup file for each profile",
                    Recommendation =
                        $"You will need to switch 'Monitors' in DCS when you switch Helios Profile",
                    Severity = StatusReportItem.SeverityCode.Info,
                    Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                };
            }
        }
    }
}