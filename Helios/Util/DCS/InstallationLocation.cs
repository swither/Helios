// Copyright 2020 Ammo Goettsch
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
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Util.DCS
{
    public class InstallationLocation : DependencyObject
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public event EventHandler ChangeEnabled;

        private const string SETTINGS_GROUP = "DCSInstallationLocations";
        public const string AUTO_UPDATE_CONFIG = "autoupdate.cfg";
        public const string DCS_EXE = "DCS.exe";

        internal static IEnumerable<InstallationLocation> ReadSettings()
        {
            // this is called in design mode, so we gracefully deal with lack of settings manager
            if (!(ConfigManager.SettingsManager is ISettingsManager2 settings))
            {
                yield break;
            }
            foreach (string path in settings.EnumerateSettingNames(SETTINGS_GROUP))
            {
                if (!TryLoadLocation(path, settings.LoadSetting(SETTINGS_GROUP, path, false), out InstallationLocation location))
                {
                    // stale setting
                    continue;
                }
                yield return location;
            }
        }


        public static bool TryBrowseLocation(string selectedFile, out InstallationLocation location)
        {
            string rootPath;
            string shortName = System.IO.Path.GetFileName(selectedFile);
            if (shortName.Equals(DCS_EXE, StringComparison.InvariantCultureIgnoreCase))
            {
                // user picked DCS.exe, maybe this is Steam or maybe they just chose to
                rootPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(selectedFile));
            }
            else
            {
                rootPath = System.IO.Path.GetDirectoryName(selectedFile);
            }

            Debug.Assert(rootPath != null,
                "a file path we retrieved from open file dialog should not have a null directory");
            
            return TryLoadLocation(rootPath, true, out location);
        }

        public static bool TryLoadLocation(string rootPath, bool enabled, out InstallationLocation location) {
            string autoUpdatePath = System.IO.Path.Combine(rootPath ?? ".", AUTO_UPDATE_CONFIG);
            string version;

            // XXX can we register for changes on these files without breaking DCS?  how does that work in Windows?
            if (File.Exists(autoUpdatePath))
            {
                string contents = File.ReadAllText(autoUpdatePath);
                try
                {
                    version = JsonConvert.DeserializeObject<AutoUpdateConfig>(contents).Version;
                }
                catch (Exception ex)
                {
                    Logger.Error("Invalid file contents: {Contents}", contents);
                    throw new Exception($"Failed to parse {AUTO_UPDATE_CONFIG}; Run DCS repair or reinstall the DCS installation in {rootPath}", ex);
                }
            }
            else {
                string exePath = System.IO.Path.Combine(rootPath ?? ".", "bin", "DCS.exe");
                if (File.Exists(exePath))
                {
                    version = ExtractFileVersion(exePath);
                }
                else
                {
                    // fail, which happens for stale settings
                    location = null;
                    return false;
                }
            }

            location = new InstallationLocation(rootPath, version, enabled);
            return true;
        }

        /// <summary>
        /// the part of the format of DCS' autoupdate.cfg file that we care about
        /// </summary>
        private class AutoUpdateConfig
        {
            #region Properties

            [JsonProperty("version")] public string Version { get; private set; }

            #endregion
        }

        /// <summary>
        /// true if we are setting properties and do not want to fire events, such as during deserialization
        /// </summary>
        private static bool _suppressEvents;

        private InstallationLocation(string path, string version, bool enabled)
        {
            _suppressEvents = true;
            try
            {
                IsEnabled = enabled;

                Path = path;
                Version = version;
                string variantFile = System.IO.Path.Combine(Path, "dcs_variant.txt");

                // check if we can write a file here
                Writable = TestWrite();

                // default saved games name
                SavedGamesName = "DCS";
                if (!File.Exists(variantFile))
                {
                    return;
                }

                string variant = File.ReadAllText(variantFile);
                variant = variant.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None)[0];
                if (!new Regex("^[A-Za-z_\\-.]+$").IsMatch(variant))
                {
                    return;
                }

                if (variant.Length > 100)
                {
                    return;
                }

                if (variant.Length < 1)
                {
                    return;
                }

                SavedGamesName = $"DCS.{variant}";
            }
            finally
            {
                _suppressEvents = false;
            }
        }

        private static string ExtractFileVersion(string exePath)
        {
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(exePath);
            return info.FileVersion;
        }

        private bool TestWrite()
        {
            try
            {
                string tempFileName = System.IO.Path.Combine(Path, System.IO.Path.GetRandomFileName());
                using (FileStream fs = File.Create(tempFileName, 1, FileOptions.DeleteOnClose))
                {
                    _ = fs;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void NotifyChangeEnabled()
        {
            ChangeEnabled?.Invoke(this, new EventArgs());
        }

        internal void DeleteSettings()
        {
            // crash if this system  has a settings manager that does not support this interface
            ISettingsManager2 settings = (ISettingsManager2) ConfigManager.SettingsManager;
            settings.DeleteSetting(SETTINGS_GROUP, Path);
        }

        internal void UpdateSettings()
        {
            ConfigManager.SettingsManager.SaveSetting(SETTINGS_GROUP, Path, IsEnabled);
        }

        private static void OnChangeEnabled(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            InstallationLocation location = (InstallationLocation) target;
            if (_suppressEvents)
            {
                return;
            }
            location.UpdateSettings();
            location.NotifyChangeEnabled();
        }

        #region Properties

        /// <summary>
        /// installation location absolute path
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// saved games name, such as "DCS" or "DCS.OpenBeta"
        /// </summary>
        public string SavedGamesName { get; internal set; }

        /// <summary>
        /// the installed version of DCS
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// if true, this location is selected for installation
        /// </summary>
        public bool IsEnabled
        {
            get => (bool) GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(InstallationLocation),
                new PropertyMetadata(true, OnChangeEnabled));

        public bool Writable { get; }

        #endregion

        #region PathComponents

        /// <summary>
        /// location of Saved Games data for this DCS instance
        /// </summary>
        public string SavedGamesPath => System.IO.Path.Combine(KnownFolders.SavedGames, SavedGamesName);

        /// <summary>
        /// location where we will write Export.lua and related directories
        /// exported for UI
        /// </summary>
        public string ScriptDirectoryPath => System.IO.Path.Combine(SavedGamesPath, "Scripts");

        public string ExportStubPath => System.IO.Path.Combine(ScriptDirectoryPath, ExportLuaName);

        public static string ExportLuaName => "Export.lua";

        public string ExportMainPath(string exportMainName) =>
            System.IO.Path.Combine(ScriptDirectoryPath, ScriptsSubdirectory, exportMainName);

        public static string ScriptsSubdirectory => "Helios";

        public string OptionsPath => System.IO.Path.Combine(SavedGamesPath, "Config", "options.lua");

        /// <summary>
        /// creates a shortened and anonymous path to the MonitorSetup folder for use in UI
        /// </summary>
        public string DescribeMonitorSetupPath =>
            // ReSharper disable once AssignNullToNotNullAttribute not possible with saved games folder
            System.IO.Path.Combine(SavedGamesTranslated, SavedGamesName, "Config", "MonitorSetup");

        private static string SavedGamesTranslated
        {
            get
            {
                string savedGamesTranslated = System.IO.Path.GetFileName(KnownFolders.SavedGames);
                return savedGamesTranslated;
            }
        }

        /// <summary>
        /// creates a shortened and anonymous path to the options.lua file for use in UI
        /// </summary>
        public string DescribeOptionsPath =>
            // ReSharper disable once AssignNullToNotNullAttribute not possible with saved games folder
            System.IO.Path.Combine(SavedGamesTranslated, SavedGamesName, "Config", "options.lua");

        public string ExportMainRelativePath(string exportMainName) =>
            System.IO.Path.Combine("Scripts", ScriptsSubdirectory, exportMainName);

        public string ExportModulePath(string moduleLocation, string baseName) =>
            System.IO.Path.Combine(ExportModuleDirectory(moduleLocation), $"{baseName}.lua");

        public string ExportModuleDirectory(string moduleLocation) => System.IO.Path.Combine(ScriptDirectoryPath, ScriptsSubdirectory, moduleLocation);

        #endregion
    }
}