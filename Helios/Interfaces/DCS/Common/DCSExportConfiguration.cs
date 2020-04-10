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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using GadrocsWorkshop.Helios.UDPInterface;
using GadrocsWorkshop.Helios.Util;
using GadrocsWorkshop.Helios.Util.DCS;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    /// <summary>
    /// This object represents two related concepts:
    /// - Site configuration for the connection between the export script and Helios (IP address, port,
    /// update rate, which files are generated and where they are placed, etc.)
    /// - Generation of export files from a particular interface object
    /// These two aspects are not decoupled, because when used as a configuration object, it
    /// constantly recomputes the export files to see if they need to be regenerated.  So any attempt
    /// to separate the export file generation from the configuration object is of limited use.
    /// </summary>
    public class DCSExportConfiguration : DCSConfiguration
    {
        // the main export script we generate
        private const string EXPORT_MAIN_NAME = "HeliosExport16.lua";

        // the interface that owns this object, and for which we generate the Exports
        private readonly DCSInterface _parent;

        /// <summary>
        /// backing field for property IsUpToDate, contains
        /// a flag that is true if exports are up to date and installed
        /// </summary>
        private bool _isUpToDate;

        /// <summary>
        /// full Lua contents of Export.lua stub we sometimes install
        /// </summary>
        private string _exportStub = "";

        /// <summary>
        /// full Lua contents of EXPORT_SCRIPT_NAME
        /// </summary>
        private string _exportMain = "";

        /// <summary>
        /// full Lua contents of driver for _parent
        /// </summary>
        private string _exportDriver = "";

        /// <summary>
        /// IP address to which Export.lua will send UDP updates
        /// </summary>
        private string _ipAddress;

        /// <summary>
        /// Export.lua update ticks per second
        /// </summary>
        private int _exportFrequency;

        /// <summary>
        /// generate Export.lua stub or just Scripts/Helios/...?
        /// </summary>
        private bool _generateExportLoader;

        /// <summary>
        /// installation type selected to choose export file location
        /// </summary>
        private DCSInstallType _selectedInstallType;

        public DCSExportConfiguration(DCSInterface parent)
        {
            _parent = parent;

            // get global configuration parameters that are not serialized to the profile
            _ipAddress = _parent.LoadSetting("IPAddress", "127.0.0.1");
            _exportFrequency = _parent.LoadSetting("ExportFrequency", 15);
            _generateExportLoader = _parent.LoadSetting("GenerateExportLoader", true);
            _selectedInstallType = _parent.LoadSetting("SelectedInstallType", DCSInstallType.Release);

            string savedDoFiles = _parent.LoadSetting("DoFiles", "");
            foreach (string file in savedDoFiles.Split(','))
            {
                if (!string.IsNullOrWhiteSpace(file))
                {
                    DoFiles.Add(file);
                }
            }

            // create our current config for configuration check
            UpdateConfiguration();

            // if the UI changes our collection of dofiles, we recompute the exports
            DoFiles.CollectionChanged += DoFiles_CollectionChanged;

            // we recompute the exports if the collection of functions in our interface ever changes, in case there are any dynamic interfaces that change functions
            _parent.Functions.CollectionChanged += Functions_CollectionChanged;

            SubscribeToLocationChanges();
        }

        public override void Dispose()
        {
            base.Dispose();
            DoFiles.CollectionChanged -= DoFiles_CollectionChanged;
            _parent.Functions.CollectionChanged -= Functions_CollectionChanged;
        }

        private void DoFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // persist all children in changed collection to a single string
            _parent.SaveSetting("DoFiles", string.Join(",", DoFiles));
            Update();
        }

        private void Functions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // some bound functions in our interface have changed
            Update();
        }

        #region Properties

        /// <summary>
        /// a flag that is true if exports are up to date and installed
        /// </summary>
        public bool IsUpToDate
        {
            get => _isUpToDate;
            set
            {
                if (_isUpToDate == value)
                {
                    return;
                }

                bool oldValue = _isUpToDate;
                _isUpToDate = value;
                OnPropertyChanged("IsUpToDate", oldValue, value, true);
            }
        }

        /// <summary>
        /// Export.lua update ticks per second
        /// This is a site-specific setting persisted in HeliosSettings instead of in the profile.
        /// </summary>
        public int ExportFrequency
        {
            get => _exportFrequency;
            set
            {
                if (!_exportFrequency.Equals(value))
                {
                    int oldValue = _exportFrequency;
                    _exportFrequency = value;
                    _parent.SaveSetting("ExportFrequency", _exportFrequency);
                    OnPropertyChanged("ExportFrequency", oldValue, value, true);
                }
            }
        }

        /// <summary>
        /// Relative paths to files that Export.lua will call via dofile(...)
        /// This is a site-specific setting persisted in HeliosSettings instead of in the profile.
        /// </summary>
        public ObservableCollection<string> DoFiles { get; } = new ObservableCollection<string>();

        /// <summary>
        /// IP address to which Export.lua will send UDP updates
        /// This is a site-specific setting persisted in HeliosSettings instead of in the profile.
        /// </summary>
        public string IPAddress
        {
            get => _ipAddress;
            set
            {
                if (_ipAddress == null && value != null
                    || _ipAddress != null && !_ipAddress.Equals(value))
                {
                    string oldValue = _ipAddress;
                    _ipAddress = value;
                    _parent.SaveSetting("IPAddress", _ipAddress);
                    OnPropertyChanged("IPAddress", oldValue, value, false);
                }
            }
        }

        /// <summary>
        /// Port number to which Export.lua will send UDP updates
        /// This is a site-specific setting persisted in HeliosSettings instead of in the profile.
        /// </summary>
        public int Port
        {
            // we want Export.lua to send to our listening port
            get => _parent.Port;
            set
            {
                int oldValue = _parent.Port;
                if (!oldValue.Equals(value))
                {
                    _parent.Port = value;
                    _parent.SaveSetting("Port", value);
                    OnPropertyChanged("Port", oldValue, value, false);
                }
            }
        }

        /// <summary>
        /// If true, we generate the Scripts/Export.lua stub in addition to the files in Scripts/Helios
        /// This is a site-specific setting persisted in HeliosSettings instead of in the profile.
        /// </summary>
        public bool GenerateExportLoader
        {
            get => _generateExportLoader;
            set
            {
                bool oldValue = _generateExportLoader;
                if (oldValue == value)
                {
                    return;
                }

                _generateExportLoader = value;
                _parent.SaveSetting("GenerateExportLoader", _generateExportLoader);
                OnPropertyChanged("GenerateExportLoader", oldValue, value, false);
            }
        }

        /// <summary>
        /// If true, this interface expects to use an externally provided export module,
        /// so we won't generate a driver and we will ask for module mode at run time.
        /// This is a profile-persisted setting via our parent.
        /// </summary>
        public bool UsesExportModule
        {
            get => _parent.UsesExportModule;
            set
            {
                bool oldValue = _parent.UsesExportModule;
                if (oldValue == value)
                {
                    return;
                }

                _parent.UsesExportModule = value;
                OnPropertyChanged("UsesExportModule", oldValue, value, false);
            }
        }

        #endregion

#if false
        public bool RestoreConfig()
        {
            string exportLuaPath = System.IO.Path.Combine(KnownFolders.SavedGames, "Scripts", "Export.lua");
            string backupFile = exportLuaPath + ".back";

            if (File.Exists(exportLuaPath))
            {
                File.Delete(exportLuaPath);
            }

            if (File.Exists(backupFile))
            {
                File.Move(backupFile, exportLuaPath);
                File.Delete(backupFile);
            }

            IsUpToDate = CheckConfig();

            return true;
        }
#endif

#if false
        string backupFile = exportLuaPath + ".back";
        if (!File.Exists(backupFile))
        {
            File.Move(exportLuaPath, backupFile);
        }
        File.Delete(exportLuaPath);

        StreamWriter propertiesFile = File.CreateText(exportLuaPath);
        propertiesFile.Write(_exportLuaStub);
        propertiesFile.Close();

        catch (Exception e)
        {
            ConfigManager.LogManager.LogError($"Could not create Export.lua '{exportLuaPath}'", e);
            throw;
        }
        IsUpToDate = CheckConfig();
#endif

#if false
        private bool CheckPath()
        {
            try
            {
                if (Directory.Exists(KnownFolders.SavedGames))
                {
                    string exportLuaPath = System.IO.Path.Combine(KnownFolders.SavedGames, "Scripts");
                    if (!Directory.Exists(exportLuaPath))
                    {
                        Directory.CreateDirectory(exportLuaPath);
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                ConfigManager.LogManager.LogError("Error checking DCS Path", e);
            }

            return false;
        }
#endif

        private void UpdateConfiguration()
        {
            UpdateDirectories();
            UpdateExportStub();
            UpdateExportScript();
            if (!UsesExportModule)
            {
                UpdateDriver();
            }
        }

        protected override void Update()
        {
            UpdateConfiguration();
            _parent.InvalidateStatusReport();
        }

        /// <summary>
        /// main entry point to write all generated files
        /// </summary>
        /// <returns>true if everything was written without error</returns>
        public override InstallationResult Install(IInstallationCallbacks callbacks)
        {
            try
            {
                List<StatusReportItem> report = new List<StatusReportItem>();
                foreach (InstallationLocation location in InstallationLocations.Singleton.Active)
                {
                    // create deepest folder
                    if (!Directory.Exists(location.ExportDriverDirectory))
                    {
                        Directory.CreateDirectory(location.ExportDriverDirectory);
                    }

                    // write all the generated files
                    File.WriteAllText(location.ExportStubPath, _exportStub);
                    report.Add(new StatusReportItem
                    {
                        Status = $"Wrote Export.Lua stub for {location.SavedGamesName}",
                        Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                    });
                    File.WriteAllText(location.ExportMainPath(EXPORT_MAIN_NAME), _exportMain);
                    report.Add(new StatusReportItem
                    {
                        Status = $"Wrote main export file {EXPORT_MAIN_NAME} for {location.SavedGamesName}",
                        Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                    });
                    if (!UsesExportModule)
                    {
                        string vehicle = _parent.ImpersonatedVehicleName ?? _parent.VehicleName;
                        File.WriteAllText(
                            location.ExportDriverPath(vehicle),
                            _exportDriver);
                        report.Add(new StatusReportItem
                        {
                            Status = $"Wrote {vehicle} export driver for {location.SavedGamesName}",
                            Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                        });
                    }
                }

                // update status after all writes
                Update();
                callbacks.Success("Configuration of exports succeeded",
                    "All configured DCS locations were updated with Export.lua and all related scripts.", report);
                return InstallationResult.Success;
            }
            catch (Exception ex)
            {
                ConfigManager.LogManager.LogError("Failed to write DCS Export scripts due to error", ex);
                callbacks.Failure("Failed to write DCS Export scripts due to error", ex.StackTrace,
                    new List<StatusReportItem>());
                return InstallationResult.Fatal;
            }
        }

        /// <summary>
        /// utility for checking if everything is up to date while editing in the UI,
        /// which is not the same as performing the ready check at run time
        /// </summary>
        /// <returns></returns>
        internal IList<StatusReportItem> CheckConfig()
        {
            List<StatusReportItem> report = new List<StatusReportItem>();
            IList<InstallationLocation> installationLocations = InstallationLocations.Singleton.Active;
            if (!installationLocations.Any())
            {
                report.Add(new StatusReportItem
                {
                    Status = "No DCS installation locations are configured for exports configuration",
                    Recommendation = "Configure any DCS installation directories you use",
                    Severity = StatusReportItem.SeverityCode.Error
                });
                IsUpToDate = false;
                return report;
            }

            // export.lua generation is optional, so we have to pick the correct function to check it
            Func<InstallationLocation, StatusReportItem> stubChecker;
            if (GenerateExportLoader)
            {
                stubChecker = CheckExportStub;
            }
            else
            {
                stubChecker = CheckThirdPartyExportStub;
            }

            foreach (InstallationLocation location in installationLocations)
            {
                List<Func<InstallationLocation, StatusReportItem>> checks =
                    new List<Func<InstallationLocation, StatusReportItem>>
                    {
                        CheckDirectories,
                        stubChecker,
                        CheckExportScript
                    };
                if (!UsesExportModule)
                {
                    checks.Add(CheckDriver);
                }

                foreach (Func<InstallationLocation, StatusReportItem> check in checks)
                {
                    StatusReportItem result = check(location);
                    report.Add(result);
                    if (!result.Flags.HasFlag(StatusReportItem.StatusFlags.ConfigurationUpToDate))
                    {
                        ConfigManager.LogManager.LogDebug(result.Status);
                        IsUpToDate = false;
                        // don't test the remaining items
                        return report;
                    }
                }
            }

            // finished
            IsUpToDate = true;
            return report;
        }

        /// <summary>
        /// main entry point for run time readiness check with a human readable narrative
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            string reportingName = _parent.ImpersonatedVehicleName ?? _parent.VehicleName;

            // XXX check if IP address is valid

            // check IP address to see if the files are are checking are actually used
            if (LocalMachineIsRemoteHelios())
            {
                yield return new StatusReportItem
                {
                    Status =
                        $"This computer may not be where DCS runs, because Exports are sent here via IP address {IPAddress}",
                    Recommendation =
                        "Helios cannot check that the export files have been copied to your computer running DCS.",
                    Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                };
            }

            IList<InstallationLocation> installationLocations = InstallationLocations.Singleton.Active;
            if (!installationLocations.Any())
            {
                yield return new StatusReportItem
                {
                    Status = "No DCS installation locations are configured for exports configuration",
                    Recommendation = "Using Helios Profile Editor, configure any DCS installation directories you use",
                    Severity = StatusReportItem.SeverityCode.Error
                };
            }

            foreach (InstallationLocation location in installationLocations)
            {
                // check on the Export.lua stub, if applicable
                if (GenerateExportLoader)
                {
                    UpdateExportStub();
                    yield return CheckExportStub(location);
                }
                else
                {
                    yield return CheckThirdPartyExportStub(location);
                }

                // check on our main script
                UpdateExportScript();
                yield return CheckExportScript(location);

                // check if drivers are generated or we expect modules copied
                if (UsesExportModule)
                {
                    yield return new StatusReportItem
                    {
                        Status = $"DCS exports for {reportingName} are provided by a third party export module.",
                        Recommendation = "Please manually check that the export module was correctly placed.",
                        Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                    };
                    if (reportingName != _parent.VehicleName)
                    {
                        yield return new StatusReportItem
                        {
                            Status =
                                $"DCS Interface for {reportingName} requires a module that maps to the {_parent.VehicleName} interface.",
                            Recommendation = "Please manually check that you are using an appropriate export module.",
                            Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                        };
                    }
                }
                else
                {
                    UpdateDriver();
                    yield return CheckDriver(location);
                }
            }
        }

        #region ExportGenerators

        private void UpdateDirectories()
        {
            // this does nothing, directories are created when we write
        }

        private void UpdateExportStub()
        {
            _exportStub =
                Resources.ReadResourceFile("pack://application:,,,/Helios;component/Interfaces/DCS/Common/Export.lua")
                + string.Join("\n", GenerateDoFileLines());
        }

        private void UpdateExportScript()
        {
            _exportMain = Resources
                .ReadResourceFile($"pack://application:,,,/Helios;component/Interfaces/DCS/Common/{EXPORT_MAIN_NAME}")
                .Replace("HELIOS_REPLACE_IPAddress", IPAddress)
                .Replace("HELIOS_REPLACE_Port", Port.ToString())
                .Replace("HELIOS_REPLACE_ExportInterval", Math.Round(1d / Math.Max(4, ExportFrequency), 3).ToString());
        }

        private void UpdateDriver()
        {
            // NOTE: if this is refactored, continue to ensure that nothing is 
            // written to _exportDriver if any component throws
            _exportDriver =
                string.Join("\n", GenerateDriverLines())
                + FetchExportFunctions()
                + "\nreturn driver";
        }

        private string GenerateFunction(DCSDataElement element) => $"[{element.ID}]=\"{element.Format}\"";

        private IEnumerable<string> GenerateFunctions(bool everyFrame)
        {
            foreach (NetworkFunction function in _parent.Functions)
            foreach (ExportDataElement element in function.GetDataElements())
            {
                if (element is DCSDataElement dcsElement)
                {
                    if (dcsElement.Format != null && dcsElement.IsExportedEveryFrame == everyFrame)
                    {
                        yield return GenerateFunction(dcsElement);
                    }
                }
            }
        }

        private IEnumerable<string> GenerateDriverLines()
        {
            yield return $"-- export driver for {_parent.VehicleName} generated by Helios Profile Editor";
            yield return "local driver = { }";
            yield return $"driver.selfName = \"{_parent.VehicleName}\"";
            yield return "driver.everyFrameArguments = { ";
            yield return $"  {string.Join(",", GenerateFunctions(true))}";
            yield return "}";
            yield return "driver.arguments = { ";
            yield return $"  {string.Join(",", GenerateFunctions(false))}";
            yield return "}";
            yield return "";
        }

        private IEnumerable<string> GenerateDoFileLines()
        {
            int index = 1;
            foreach (string file in DoFiles)
            {
                // generate a local to hold a reference to anything the dofile returns, to
                // ensure its lifetime and report result in case of error
                yield return $"local helios_dofile_{index} = {{ }}";
                yield return $"helios_dofile_{index}.success, helios_dofile_{index}.result = pcall(dofile, \"{file}\")";
                yield return
                    $"if not helios_dofile_{index}.success then log.write(\"HELIOS.EXPORT\", log.ERROR, string.format(\"error return from configured external '{file}': %s\", tostring(helios_dofile_{index}.result))) end";
                index++;
            }
        }

        private string FetchExportFunctions()
        {
            if (string.IsNullOrWhiteSpace(_parent.ExportFunctionsPath))
            {
                return "";
            }

            try
            {
                return Resources.ReadResourceFile(_parent.ExportFunctionsPath);
            }
            catch (Exception ex)
            {
                ConfigManager.LogManager.LogError(
                    $"failed to load interface-specific functions for {_parent.VehicleName} from '{_parent.ExportFunctionsPath}'",
                    ex);
                throw;
            }
        }

        #endregion

        #region ExportChecks

        private StatusReportItem CheckDirectories(InstallationLocation location)
        {
            string path = location.ScriptDirectoryPath;
            if (!Directory.Exists(path))
            {
                return new StatusReportItem
                {
                    Status = $"Export scripts have not been generated for {location.SavedGamesName}",
                    Recommendation =
                        "Using Helios Profile Editor, configure the DCS interface to generate export files",
                    Severity = StatusReportItem.SeverityCode.Error
                };
            }

            return new StatusReportItem
            {
                Status = $"Export scripts location for {location.SavedGamesName} exists",
                Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate | StatusReportItem.StatusFlags.Verbose
            };
        }

        // NOTE: we don't factor this to share code with the other Check... functions because they are all
        // subtly different in terms of severity and we want to encourage more special cases added in the future
        private StatusReportItem CheckExportStub(InstallationLocation location)
        {
            string exportLuaPath = location.ExportStubPath;
            if (!File.Exists(exportLuaPath))
            {
                return new StatusReportItem
                {
                    Status = $"The configured DCS Export.lua stub does not exist at '{exportLuaPath}'",
                    Recommendation =
                        "Using Helios Profile Editor, configure the DCS interface or configure install location correctly to locate the file.",
                    // survive this
                    Severity = StatusReportItem.SeverityCode.Warning
                };
            }

            // NOTE: we have the entire contents we expect in this file in memory, so there is no point in hashing this
            string contents = File.ReadAllText(exportLuaPath);
            if (!contents.Equals(_exportStub))
            {
                return new StatusReportItem
                {
                    Status = $"The DCS Export.lua stub at '{exportLuaPath}' does not match configuration",
                    Recommendation = "Using Helios Profile Editor, configure the DCS interface.",
                    // survive this
                    Severity = StatusReportItem.SeverityCode.Warning
                };
            }

            return new StatusReportItem
            {
                Status = $"The configured DCS Export.lua stub at '{exportLuaPath}' is up to date.",
                Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
            };
        }

        // NOTE: we don't factor this to share code with the other Check... functions because they are all
        // subtly different in terms of severity and we want to encourage more special cases added in the future
        private StatusReportItem CheckThirdPartyExportStub(InstallationLocation location)
        {
            string exportLuaPath = location.ExportStubPath;
            if (!File.Exists(exportLuaPath))
            {
                return new StatusReportItem
                {
                    Status = $"The Export.lua stub does not exist at '{exportLuaPath}'",
                    Recommendation = "Using Helios Profile Editor, generate Export.lua or create it manually.",
                    Severity = StatusReportItem.SeverityCode.Error,
                    // writing updates won't fix this
                    Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                };
            }

            string contents = File.ReadAllText(exportLuaPath);
            if (!contents.Contains(EXPORT_MAIN_NAME))
            {
                return new StatusReportItem
                {
                    Status =
                        $"Helios Export script Helios\\{EXPORT_MAIN_NAME} does not appear to be called by Export.lua at '{exportLuaPath}'",
                    Recommendation = "Using Helios Profile Editor, recreate Export.lua or edit it manually.",
                    Severity = StatusReportItem.SeverityCode.Error,
                    // writing updates won't fix this
                    Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                };
            }

            return new StatusReportItem
            {
                Status = $"The Export.lua stub at '{exportLuaPath}' is not generated by Helios.",
                Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
            };
        }

        // NOTE: we don't factor this to share code with the other Check... functions because they are all
        // subtly different in terms of severity and we want to encourage more special cases added in the future
        private StatusReportItem CheckExportScript(InstallationLocation location)
        {
            string mainPath = location.ExportMainPath(EXPORT_MAIN_NAME);
            if (!File.Exists(mainPath))
            {
                return new StatusReportItem
                {
                    Status = $"The configured DCS export script does not exist at '{mainPath}'",
                    Recommendation =
                        "Using Helios Profile Editor, configure the DCS interface or configure install locations correctly to locate the file.",
                    Severity = StatusReportItem.SeverityCode.Error
                };
            }

            // NOTE: we have the entire contents we expect in this file in memory, so there is no point in hashing this
            string contents = File.ReadAllText(mainPath);
            if (!contents.Equals(_exportMain))
            {
                return new StatusReportItem
                {
                    Status = $"The DCS export script at '{mainPath}' does not match configuration",
                    Recommendation = "Using Helios Profile Editor, configure the DCS interface.",
                    Severity = StatusReportItem.SeverityCode.Error
                };
            }

            return new StatusReportItem
            {
                Status = $"The configured DCS export script at '{mainPath}' is up to date.",
                Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
            };
        }

        // NOTE: we don't factor this to share code with the other Check... functions because they are all
        // subtly different in terms of severity and we want to encourage more special cases added in the future
        private StatusReportItem CheckDriver(InstallationLocation location)
        {
            string exportDriverPath = location.ExportDriverPath(_parent.ImpersonatedVehicleName ?? _parent.VehicleName);
            if (!File.Exists(exportDriverPath))
            {
                return new StatusReportItem
                {
                    Status = $"The driver generated by Helios Profile Editor does not exist at '{exportDriverPath}'",
                    Recommendation =
                        "Using Helios Profile Editor, generate the file or configure DCS install type correctly to locate the file.",
                    Severity = StatusReportItem.SeverityCode.Error
                };
            }

            // NOTE: we have the entire contents we expect in this file in memory, so there is no point in hashing this
            string contents = File.ReadAllText(exportDriverPath);
            if (!contents.Equals(_exportDriver))
            {
                return new StatusReportItem
                {
                    Status =
                        $"The driver generated by Helios Profile Editor at '{exportDriverPath}' does not match configuration",
                    Recommendation =
                        $"Using Helios Profile Editor, select the interface for {_parent.VehicleName} and run the DCS setup.",
                    Severity = StatusReportItem.SeverityCode.Error
                };
            }

            return new StatusReportItem
            {
                Status = $"The driver generated by Helios Profile Editor at '{exportDriverPath}' is up to date.",
                Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
            };
        }

        // XXX if address is localhost, false
        // XXX if address is broadcast or multicast, true
        // XXX if address is address of one of our adapters, true
        private bool LocalMachineIsRemoteHelios() => false;

        #endregion

#if false
        string backupFile = exportLuaPath + ".back";
        if (!File.Exists(backupFile))
        {
            File.Move(exportLuaPath, backupFile);
        }
        File.Delete(exportLuaPath);

        StreamWriter propertiesFile = File.CreateText(exportLuaPath);
        propertiesFile.Write(_exportLuaStub);
        propertiesFile.Close();

        catch (Exception e)
        {
            ConfigManager.LogManager.LogError($"Could not create Export.lua '{exportLuaPath}'", e);
            throw;
        }
        IsUpToDate = CheckConfig();
#endif
    }
}