using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;

namespace GadrocsWorkshop.Helios.Util.DCS
{
    public class InstallationLocation : DependencyObject
    {
        public event System.EventHandler ChangeEnabled;

        /// <summary>
        /// the part of the format of DCS' autoupdate.cfg file that we care about
        /// </summary>
        private class AutoUpdateConfig
        {
            [JsonProperty("version")]
            public string Version { get; private set; }
        }

        public InstallationLocation(string autoUpdatePath)
        {
            Path = System.IO.Path.GetDirectoryName(autoUpdatePath);

            // XXX can we register for changes on these files without breaking DCS?  how does that work in Windows?
            Version = JsonConvert.DeserializeObject<AutoUpdateConfig>(System.IO.File.ReadAllText(autoUpdatePath)).Version;
            string variantFile = System.IO.Path.Combine(Path, "dcs_variant.txt");

            // default saved games name
            SavedGamesName = "DCS";
            if (!System.IO.File.Exists(variantFile))
            {
                return;
            }
            string variant = System.IO.File.ReadAllText(variantFile);
            variant = variant.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None)[0];
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

        public string Path { get; private set; }
        public string Version { get; private set; }
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        public string SavedGamesName { get; internal set; }

        private const string SETTINGS_GROUP = "DCSInstallationLocations";
        public const string AUTO_UPDATE_CONFIG = "autoupdate.cfg";

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(InstallationLocation), new PropertyMetadata(true, OnChangeEnabled));

        private static void OnChangeEnabled(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            InstallationLocation location = ((InstallationLocation)target);
            location.UpdateSettings();
            location.NotifyChangeEnabled();
        }

        private void NotifyChangeEnabled()
        {
            ChangeEnabled?.Invoke(this, new System.EventArgs());
        }

        internal static IEnumerable<InstallationLocation> ReadSettings()
        {
            // crash if this system  has a settings manager that does not support this interface
            ISettingsManager2 settings = (ISettingsManager2)ConfigManager.SettingsManager;
            foreach (string path in settings.EnumerateSettingNames(SETTINGS_GROUP))
            {
                InstallationLocation location = new InstallationLocation(System.IO.Path.Combine(path, AUTO_UPDATE_CONFIG));
                location.IsEnabled = settings.LoadSetting(SETTINGS_GROUP, path, false);
                yield return location;
            }
        }

        internal void DeleteSettings()
        {
            // crash if this system  has a settings manager that does not support this interface
            ISettingsManager2 settings = (ISettingsManager2)ConfigManager.SettingsManager;
            settings.DeleteSetting(SETTINGS_GROUP, Path);
        }

        internal void UpdateSettings()
        {
            ConfigManager.SettingsManager.SaveSetting(SETTINGS_GROUP, Path, IsEnabled);
        }
    }
}