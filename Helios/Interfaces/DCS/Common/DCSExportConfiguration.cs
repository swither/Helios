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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    using GadrocsWorkshop.Helios.Util;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;

    /// <summary>
    /// This object represents two related concepts:
    /// 
    /// - Site configuration for the connection between the export script and Helios (IP address, port,
    ///   update rate, which files are generated and where they are placed, etc.)
    ///   
    /// - Generation of export files from a particular interface object
    /// 
    /// These two aspects are not decoupled, because when used as a configuration object, it 
    /// constantly recomputes the export files to see if they need to be regenerated.  So any attempt
    /// to separate the export file generation from the configuration object is of limited use.
    /// 
    /// It is instantiated in two different contexts:  The Profile Editor instantiates it as a component
    /// of the DCS Interface Editor, which is why this is a NotificationObject for light-weight UI support.
    /// Secondly, it is instantiated by the Control Center before running a Profile to check the Export
    /// configuration as a pre-flight check.
    /// </summary>
    public class DCSExportConfiguration: NotificationObject, IReadyCheck
    {
        // the main export script we generate
        private const string EXPORT_SCRIPT_NAME = "HeliosExport16.lua";

        // the interface that owns this object, and for which we generate the Exports
        private readonly DCSInterface _parent;

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
        /// false if we need to write any files
        /// </summary>
        private bool _isUpToDate = false;

        /// <summary>
        /// IP address to which Export.lua will send UDP updates
        /// </summary>
        private string _ipAddress = "127.0.0.1";

        /// <summary>
        /// Export.lua update ticks per second
        /// </summary>
        private int _exportFrequency = 15;

        /// <summary>
        /// generate Export.lua stub or just Scripts/Helios/...?
        /// </summary>
        private bool _generateExportLoader;

        /// <summary>
        /// installation type selected to choose export file location
        /// </summary>
        private DCSInstallType _selectedInstallType;

        /// <summary>
        /// extended status result that also carries the up to date
        /// result, so we can use the same calls for ready check and 
        /// checking for up to date at configuration time
        /// </summary>
        private class CheckResult: StatusReportItem
        {
            public bool IsUpToDate { get; internal set; }
        }

        public DCSExportConfiguration(DCSInterface parent) 
        {
            _parent = parent;

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

            // if the UI changes our collection of dofiles, we recompute the exports
            DoFiles.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(DoFiles_CollectionChanged);

            // we recompute the exports if the collection of functions in our interface ever changes, in case there are any dynamic interfaces that change functions
            _parent.Functions.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Functions_CollectionChanged);
        }

        void DoFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Save Config
            _parent.SaveSetting("DoFiles", string.Join(",", DoFiles));
            UpdateExports();
        }

        #region Properties

        /// <summary>
        /// Export.lua update ticks per second
        /// 
        /// This is a site-specific setting persisted in HeliosSettings instead of in the profile.
        /// </summary>
        public int ExportFrequency
        {
            get
            {
                return _exportFrequency;
            }
            set
            {
                if (!_exportFrequency.Equals(value))
                {
                    int oldValue = _exportFrequency;
                    _exportFrequency = value;
                    _parent.SaveSetting("ExportFrequency", _exportFrequency);
                    OnPropertyChanged("ExportFrequency", oldValue, value, true);
                    UpdateExports();
                }
            }
        }

        /// <summary>
        /// Relative paths to files that Export.lua will call via dofile(...)
        /// 
        /// This is a site-specific setting persisted in HeliosSettings instead of in the profile.
        /// </summary>
        public ObservableCollection<string> DoFiles { get; } = new ObservableCollection<string>();

        /// <summary>
        /// IP address to which Export.lua will send UDP updates
        /// 
        /// This is a site-specific setting persisted in HeliosSettings instead of in the profile.
        /// </summary>
        public string IPAddress
        {
            get
            {
                return _ipAddress;
            }
            set
            {
                if ((_ipAddress == null && value != null)
                    || (_ipAddress != null && !_ipAddress.Equals(value)))
                {
                    string oldValue = _ipAddress;
                    _ipAddress = value;
                    _parent.SaveSetting("IPAddress", _ipAddress);
                    OnPropertyChanged("IPAddress", oldValue, value, false);
                    UpdateExports();
                }
            }
        }

        /// <summary>
        /// Port number to which Export.lua will send UDP updates
        /// 
        /// This is a site-specific setting persisted in HeliosSettings instead of in the profile.
        /// </summary>
        public int Port
        {
            get
            {
                // we want Export.lua to send to our listening port
                return _parent.Port;
            }
            set
            {
                int oldValue = _parent.Port;
                _parent.Port = value;
                if (!oldValue.Equals(value))
                {
                    OnPropertyChanged("Port", oldValue, value, false);
                    // persist
                    _parent.SaveSetting("Port", value);
                    // invalidate export script
                    UpdateExports();
                }
            }
        }

        /// <summary>
        /// If false, we need to write new export scripts
        /// 
        /// This is a site-specific setting persisted in HeliosSettings instead of in the profile.
        /// </summary>
        public bool IsUpToDate
        {
            get
            {
                return _isUpToDate;
            }
            private set
            {
                if (!_isUpToDate.Equals(value))
                {
                    bool oldValue = _isUpToDate;
                    _isUpToDate = value;
                    OnPropertyChanged("IsUpToDate", oldValue, value, false);
                }
            }
        }

        /// <summary>
        /// If true, we generate the Scripts/Export.lua stub in addition to the files in Scripts/Helios
        /// 
        /// This is a site-specific setting persisted in HeliosSettings instead of in the profile.
        /// </summary>
        public bool GenerateExportLoader { 
            get => _generateExportLoader; 
            set
            {
                bool oldValue = _generateExportLoader;
                if (oldValue == value)
                {
                    return;
                }
                _generateExportLoader = value;
                if (value && !oldValue)
                {
                    // need to generate extra file
                    IsUpToDate = false;
                }
                OnPropertyChanged("GenerateExportLoader", oldValue, value, false);
                _parent.SaveSetting("GenerateExportLoader", _generateExportLoader);
            }
        }

        /// <summary>
        /// If true, this interface expects to use an externally provided export module,
        /// so we won't generate a driver and we will ask for module mode at run time.
        /// 
        /// This is a profile-persisted setting.
        /// </summary>
        public bool UsesExportModule {
            get => _parent.UsesExportModule; 
            set { 
                bool oldValue = _parent.UsesExportModule;
                if (oldValue == value)
                {
                    return;
                }
                _parent.UsesExportModule = value;
                if (oldValue && !value)
                {
                    // need to generate driver
                    IsUpToDate = false;
                }
                OnPropertyChanged("UsesExportModule", oldValue, value, false);
            } 
        }

        /// <summary>
        /// location where we will write Export.lua and related directories
        /// exported for UI
        /// </summary>
        public string ScriptDirectoryPath
        {
            get => Path.Combine(KnownFolders.SavedGames, SavedGamesName, "Scripts");
        }

        /// <summary>
        /// selected install type 
        /// </summary>
        public DCSInstallType SelectedInstallType
        {
            get => _selectedInstallType;
            set {
                DCSInstallType oldValue = _selectedInstallType;
                _selectedInstallType = value;
                if (oldValue != _selectedInstallType)
                {
                    _parent.SaveSetting("SelectedInstallType", _selectedInstallType);
                    string oldPath = ScriptDirectoryPath;
                    OnPropertyChanged("SelectedInstallType", oldValue, value, false);
                    OnPropertyChanged("ScriptDirectoryPath", oldPath, ScriptDirectoryPath, false);
                }
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

        #region PathComponents
        private string SavedGamesName
        {
            get
            {
                switch (SelectedInstallType)
                {
                    case DCSInstallType.OpenAlpha:
                        return "DCS.OpenAlpha";
                    case DCSInstallType.OpenBeta:
                        return "DCS.OpenBeta";
                    case DCSInstallType.Release:
                    default:
                        return "DCS";
                }
            }
        }

        private string ExportStubPath => Path.Combine(ScriptDirectoryPath, "Export.lua");

        private string ExportMainPath => Path.Combine(ScriptDirectoryPath, "Helios", EXPORT_SCRIPT_NAME);
        
        private string ExportDriverPath => Path.Combine(ExportDriverDirectory, $"{_parent.VehicleName}.lua");

        private string ExportDriverDirectory => Path.Combine(ScriptDirectoryPath, "Helios", "Drivers");
        #endregion

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

        /// <summary>
        /// main entry point for generating all things that may be out of date
        /// </summary>
        public void UpdateExports()
        {
            UpdateDirectories();
            UpdateExportStub();
            UpdateExportScript();
            UpdateDriver();
            IsUpToDate = CheckConfig();
        }

        /// <summary>
        /// main entry point to write all generated files
        /// </summary>
        /// <returns>true if everything was written without error</returns>
        public bool WriteExports()
        {
            try
            {
                // create deepest folder
                if (!Directory.Exists(ExportDriverDirectory))
                {
                    Directory.CreateDirectory(ExportDriverDirectory);
                }

                // write all the generated files
                File.WriteAllText(ExportStubPath, _exportStub);
                File.WriteAllText(ExportMainPath, _exportMain);
                File.WriteAllText(ExportDriverPath, _exportDriver);

                // all up to date
                IsUpToDate = true;
                return true;
            }
            catch (System.Exception ex)
            {
                ConfigManager.LogManager.LogError("Failed to write DCS Export scripts due to error", ex);
                return false;
            }
        }

        /// <summary>
        /// main entry point for checking if everything is up to date
        /// </summary>
        /// <returns></returns>
        public bool CheckConfig()
        {
            foreach (System.Func<CheckResult> check in new System.Func<CheckResult>[] { 
                CheckDirectories,
                CheckExportStub, 
                CheckExportScript,
                CheckDriver}) 
            {
                CheckResult result = check();
                if (!result.IsUpToDate)
                {
                    ConfigManager.LogManager.LogDebug(result.Status);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// main entry point for run time readiness check with a human readable narrative
        /// </summary>
        /// <returns></returns>
        public IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            string reportingName = _parent.ImpersonatedVehicleName ?? _parent.VehicleName;

            // XXX check if IP address is valid

            // check IP address to see if the files are are checking are actually used
            if (LocalMachineIsRemoteHelios())
            {
                yield return new StatusReportItem()
                {
                    Status = $"This computer may not be where DCS runs, because Exports are sent here via IP address {IPAddress}",
                    Recommendation = "Please manually copy export files to your computer running DCS.\nHelios cannot check that this was done."
                };
            }

            // check on the Export.lua stub, if applicable
            if (GenerateExportLoader)
            {
                UpdateExportStub();
                yield return CheckExportStub();
            }
            else
            {
                yield return CheckThirdPartyExportStub();
            }

            // check on our main script
            UpdateExportScript();
            yield return CheckExportScript();

            // check if drivers are generated or we expect modules copied
            if (UsesExportModule)
            {
                yield return new StatusReportItem()
                {
                    Status = $"DCS exports for {reportingName} are provided by third party export module.",
                    Recommendation = "Please manually check that this file was correctly placed."
                };
                if (reportingName != _parent.VehicleName)
                {
                    yield return new StatusReportItem()
                    {
                        Status = $"DCS Interface for {reportingName} requires a module that maps to the {_parent.VehicleName} interface.",
                        Recommendation = "Please manually check that this file was correctly placed."
                    };
                }
            }
            else
            {
                UpdateDriver();
                yield return CheckDriver();
            }
        }

        #region ExportGenerators
        private void UpdateDirectories()
        {
            // this does nothing, directories are created when we write
        }

        private void UpdateExportStub()
        {
            _exportStub = Resources.ReadResourceFile("pack://application:,,,/Helios;component/Interfaces/DCS/Common/Export.lua")
                + string.Join("\n", GenerateDoFileLines());
        }

        private void UpdateExportScript()
        {
            _exportMain = Resources.ReadResourceFile($"pack://application:,,,/Helios;component/Interfaces/DCS/Common/{EXPORT_SCRIPT_NAME}")
                .Replace("HELIOS_REPLACE_IPAddress", IPAddress)
                .Replace("HELIOS_REPLACE_Port", Port.ToString())
                .Replace("HELIOS_REPLACE_ExportInterval", System.Math.Round(1d / System.Math.Max(4, ExportFrequency), 3).ToString());
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

        private string GenerateFunction(DCSDataElement element)
        {
            return $"[{element.ID}]=\"{element.Format}\"";
        }

        private IEnumerable<string> GenerateFunctions(bool everyFrame)
        {
            foreach (UDPInterface.NetworkFunction function in _parent.Functions)
            {
                foreach (UDPInterface.ExportDataElement element in function.GetDataElements())
                {
                    if (element is DCSDataElement dcsElement)
                    {
                        if ((dcsElement.Format != null) && (dcsElement.IsExportedEveryFrame == everyFrame))
                        {
                            yield return GenerateFunction(dcsElement);
                        }
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
                yield return $"if not helios_dofile_{index}.success then log.write(\"HELIOS.EXPORT\", log.ERROR, string.format(\"error return from configured external '{file}': %s\", tostring(helios_dofile_{index}.result))) end";
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
            catch (System.Exception ex)
            {
                ConfigManager.LogManager.LogError($"failed to load interface-specific functions for {_parent.VehicleName} from '{_parent.ExportFunctionsPath}'", ex);
                throw;
            }
        }
        #endregion

        #region ExportChecks

        private CheckResult CheckDirectories()
        {
            return new CheckResult()
            {
                Status = "XXX unimplemented",
                IsUpToDate = true
            };
        }

        // NOTE: we don't factor this to share code with the other Check... functions because they are all
        // subtly different in terms of severity and we want to encourage more special cases added in the future
        private CheckResult CheckExportStub()
        {
            string exportLuaPath = ExportStubPath;
            if (!File.Exists(exportLuaPath))
            {
                return new CheckResult()
                {
                    Status = $"The configured DCS Export.lua stub does not exist at\n'{exportLuaPath}'",
                    Recommendation = "Using Helios Profile Editor, generate the file or configure DCS install type correctly to locate the file.",
                    // survive this
                    Severity = StatusReportItem.SeverityCode.Warning
                };
            }

            // NOTE: we have the entire contents we expect in this file in memory, so there is no point in hashing this
            string contents = File.ReadAllText(exportLuaPath);
            if (!contents.Equals(_exportStub))
            {
                return new CheckResult()
                {
                    Status = $"The DCS Export.lua stub at '{exportLuaPath}' does not match configuration",
                    Recommendation = "Using Helios Profile Editor, recreate the file or configure DCS interface not to create it.",
                    // survive this
                    Severity = StatusReportItem.SeverityCode.Warning
                };
            }

            return new CheckResult()
            {
                Status = $"The configured DCS Export.lua stub at\n'{exportLuaPath}' is up to date.",
                IsUpToDate = true
            };
        }

        private CheckResult CheckThirdPartyExportStub()
        {
            string exportLuaPath = ExportStubPath;
            if (!File.Exists(exportLuaPath))
            {
                return new CheckResult()
                {
                    Status = $"The Export.lua stub does not exist at\n'{exportLuaPath}'",
                    Recommendation = "Using Helios Profile Editor, configure Helios to create this file or place it there manually.",
                    Severity = StatusReportItem.SeverityCode.Error,
                    // writing updates won't fix this
                    IsUpToDate = true
                };
            }

            string contents = File.ReadAllText(exportLuaPath);
            if (!contents.Contains(EXPORT_SCRIPT_NAME))
            {
                return new CheckResult()
                {
                    Status = $"Helios Export script Helios\\{EXPORT_SCRIPT_NAME} does not appear to be called by Export.lua at\n'{exportLuaPath}'",
                    Recommendation = "Using Helios Profile Editor, configure Helios to create this file or edit it manually.",
                    Severity = StatusReportItem.SeverityCode.Error,
                    // writing updates won't fix this
                    IsUpToDate = true
                };
            }

            return new CheckResult()
            {
                Status = $"The Export.lua stub at\n'{exportLuaPath}' is not generated by Helios.",
                IsUpToDate = true
            };
        }

        private CheckResult CheckExportScript()
        {
            string mainPath = ExportMainPath;
            if (!File.Exists(mainPath))
            {
                return new CheckResult()
                {
                    Status = $"The configured DCS export script does not exist at\n'{mainPath}'",
                    Recommendation = "Using Helios Profile Editor, generate the file or configure DCS install type correctly to locate the file.",
                    Severity = StatusReportItem.SeverityCode.Error
                };
            }

            // NOTE: we have the entire contents we expect in this file in memory, so there is no point in hashing this
            string contents = File.ReadAllText(mainPath);
            if (!contents.Equals(_exportMain))
            {
                return new CheckResult()
                {
                    Status = $"The DCS export script at '{mainPath}' does not match configuration",
                    Recommendation = "Using Helios Profile Editor, recreate the file.",
                    Severity = StatusReportItem.SeverityCode.Error
                };
            }

            return new CheckResult()
            {
                Status = $"The configured DCS export script at\n'{mainPath}' is up to date.",
                IsUpToDate = true
            };
        }

        private CheckResult CheckDriver()
        {
            string exportDriverPath = ExportDriverPath;
            if (!File.Exists(exportDriverPath))
            {
                return new CheckResult()
                {
                    Status = $"The driver generated by Helios Profile Editor does not exist at\n'{exportDriverPath}'",
                    Recommendation = "Using Helios Profile Editor, generate the file or configure DCS install type correctly to locate the file.",
                    Severity = StatusReportItem.SeverityCode.Error
                };
            }

            // NOTE: we have the entire contents we expect in this file in memory, so there is no point in hashing this
            string contents = File.ReadAllText(exportDriverPath);
            if (!contents.Equals(_exportDriver))
            {
                return new CheckResult()
                {
                    Status = $"The driver generated by Helios Profile Editor at '{exportDriverPath}' does not match configuration",
                    Recommendation = $"Using Helios Profile Editor, select the interface for {_parent.VehicleName} and run the DCS setup.",
                    Severity = StatusReportItem.SeverityCode.Error
                };
            }

            return new CheckResult()
            {
                Status = $"The driver generated by Helios Profile Editor at\n'{exportDriverPath}' is up to date.",
                IsUpToDate = true
            };
        }

        private bool LocalMachineIsRemoteHelios()
        {
            // XXX if address is localhost, false
            // XXX if address is broadcast or multicast, true
            // XXX if address is address of one of our adapters, true
            return false;
        }
        #endregion

        void Functions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // some bound functions in our interface have changed
            UpdateExports();
        }

        void Interface_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // some properties in our interface have changed
            UpdateExports();
        }

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
