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
    using GadrocsWorkshop.Helios.UDPInterface;
    using GadrocsWorkshop.Helios.Util;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Text;

    public class DCSExportConfiguration : NotificationObject, IReadyCheck
    {
        // the interface that owns this object, and for which we generate the Exports
        private readonly DCSInterface _parent;
        private string _heliosExportLUA = "";
        private string _heliosExportLUAHash = "";
        private bool _isConfigUpToDate = false;

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

        public DCSExportConfiguration(DCSInterface parent) 
        {
            _parent = parent;
            _heliosExportLUA = "pack://application:,,,/Helios;component/Interfaces/DCS/Scripts/Common/HeliosExport16.lua";

            _ipAddress = _parent.LoadSetting("IPAddress", "127.0.0.1");
            _exportFrequency = _parent.LoadSetting("ExportFrequency", 15);
            _generateExportLoader = _parent.LoadSetting("GenerateExportLoader", true);

            string savedDoFiles = _parent.LoadSetting("DoFiles", "");
            foreach (string file in savedDoFiles.Split(','))
            {
                if (!string.IsNullOrWhiteSpace(file))
                {
                    DoFiles.Add(file);
                }
            }

            UpdateHeliosProperties();

            DoFiles.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(DoFiles_CollectionChanged);
        }

        void DoFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Save Config
            _parent.SaveSetting("DoFiles", string.Join(",", DoFiles));
            UpdateHeliosProperties();
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
                    UpdateHeliosProperties();
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
                    UpdateHeliosProperties();
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
                    UpdateHeliosProperties();
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
                return _isConfigUpToDate;
            }
            private set
            {
                if (!_isConfigUpToDate.Equals(value))
                {
                    bool oldValue = _isConfigUpToDate;
                    _isConfigUpToDate = value;
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

        #endregion
        public bool UpdateExportConfig()
        {
            IsUpToDate = true;
            return true;

            // XXX rewrite
            string exportLuaPath = System.IO.Path.Combine(KnownFolders.SavedGames, "Scripts", "Export.lua");
            try
            {
                if (File.Exists(exportLuaPath))
                {
                    string currentHash = Hash.GetMD5HashFromFile(exportLuaPath);
                    if (!currentHash.Equals(_heliosExportLUAHash))
                    {
                        string backupFile = exportLuaPath + ".back";
                        if (!File.Exists(backupFile))
                        {
                            File.Move(exportLuaPath, backupFile);
                        }
                    }
                    File.Delete(exportLuaPath);
                }

                StreamWriter propertiesFile = File.CreateText(exportLuaPath);
                propertiesFile.Write(_heliosExportLUA);
                propertiesFile.Close();
               
            }
            catch (Exception e)
            {
                ConfigManager.LogManager.LogError("DCS Configuration - Error updating Export.lua (Filename=\"" + exportLuaPath + "\")", e);
                return false;
            }

            IsUpToDate = CheckConfig();

            return true;
        }

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

        private bool CheckConfig()
        {
            return CheckConfig("Export.lua");
        }

        private bool CheckConfig(String exportFile)
        {
            return false;

            // XXX rewrite
            try
            {

                string exportLuaPath = System.IO.Path.Combine(KnownFolders.SavedGames, "Scripts", exportFile);
                return CheckFile(exportLuaPath, _heliosExportLUAHash);
            }
            catch (Exception e)
            {
                ConfigManager.LogManager.LogError("Error checking status of DCS Export configuration", e);
            }

            return false;
        }

        private bool CheckFile(string path, string expectedHash)
        {
            if (File.Exists(path))
            {
                string currentExportHash = Hash.GetMD5HashFromFile(path);

                if (currentExportHash.Equals(expectedHash))
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateHeliosProperties()
        {
            return;


            // XXX rewrite
            string _luaVarScope = "local l"; 
            double exportInterval = Math.Round(1d / Math.Max(4, _exportFrequency), 3);
            int lowExportTickInterval = Math.Min(1, (int)Math.Floor(0.250d / exportInterval));

            StringWriter configFile = new StringWriter();
            string aircraft = "unknown";
            foreach(string tag in _parent.Tags)
            {
                aircraft = tag;
                // only actually need the first one for now
                break;
            }
            configFile.WriteLine("local l" + "Aircraft = \"" + aircraft + "\"");

            configFile.WriteLine(_luaVarScope + "Host = \"" + IPAddress + "\"");
            configFile.WriteLine(_luaVarScope + "Port = " + Port.ToString(CultureInfo.InvariantCulture));
            configFile.WriteLine(_luaVarScope + "ExportInterval = " + exportInterval.ToString(CultureInfo.InvariantCulture));
            configFile.WriteLine(_luaVarScope + "ExportLowTickInterval = " + lowExportTickInterval.ToString(CultureInfo.InvariantCulture));
            bool addEveryFrameComma = false;
            bool addComma = false;
            StringBuilder everyFrameArguments = new StringBuilder();
            StringBuilder arguments = new StringBuilder();

            foreach (NetworkFunction function in _parent.Functions)
            {
                foreach (ExportDataElement element in function.GetDataElements())
                {
                    DCSDataElement dcsElement = element as DCSDataElement;
                    if (dcsElement != null && dcsElement.Format != null)
                    {
                        if (dcsElement.IsExportedEveryFrame)
                        {
                            if (addEveryFrameComma)
                            {
                                everyFrameArguments.Append(", ");
                            }
                            everyFrameArguments.Append("[");
                            everyFrameArguments.Append(dcsElement.ID);
                            everyFrameArguments.Append("]=\"");
                            everyFrameArguments.Append(dcsElement.Format);
                            everyFrameArguments.Append("\"");
                            addEveryFrameComma = true;
                        }
                        else
                        {
                            if (addComma)
                            {
                                arguments.Append(", ");
                            }
                            arguments.Append("[");
                            arguments.Append(dcsElement.ID);
                            arguments.Append("]=\"");
                            arguments.Append(dcsElement.Format);
                            arguments.Append("\"");
                            addComma = true;
                        }
                    }
                }
            }

            configFile.Write(_luaVarScope + "EveryFrameArguments = {");
            configFile.Write(everyFrameArguments.ToString());
            configFile.WriteLine("}");

            configFile.Write(_luaVarScope + "Arguments = {");
            configFile.Write(arguments.ToString());
            configFile.WriteLine("}");

            configFile.WriteLine("");

            if (!string.IsNullOrWhiteSpace(_parent.ExportFunctionsPath))
            {
                Resources.CopyResourceFile(_parent.ExportFunctionsPath, configFile);
            }

            configFile.WriteLine("");

            foreach (string file in DoFiles)
            {
                configFile.Write("dofile(\"");
                configFile.Write(file);
                configFile.WriteLine("\")");
            }

            configFile.Close();

            _heliosExportLUA = configFile.ToString();
            _heliosExportLUAHash = Hash.GetMD5HashFromString(_heliosExportLUA);

            IsUpToDate = CheckConfig();
        }

        void Functions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // some bound functions in our interface have changed
            UpdateHeliosProperties();
        }

        void Interface_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // some properties in our interface have changed
            UpdateHeliosProperties();
        }

        public IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            List<StatusReportItem> items = new List<StatusReportItem>();
            string reportingName = _parent.ImpersonatedVehicleName ?? _parent.VehicleName;
            if (UsesExportModule)
            {
                // XXX can still check the main scripts (Export.lua and Helios\HeliosExports??.lua)
            }
            items.Add(new StatusReportItem()
            {
                Status = $"DCS Interface for {reportingName} would report on status of export script here.",
                Severity = StatusReportItem.SeverityCode.Error,
                Recommendation = $"XXX Please implement the rest of DCSExportConfiguration"
            }); ;
            return items;
        }
    }
}
