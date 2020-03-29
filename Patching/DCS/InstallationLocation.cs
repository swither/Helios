using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows;

namespace GadrocsWorkshop.Helios.Patching.DCS
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
            Version = JsonConvert.DeserializeObject<AutoUpdateConfig>(System.IO.File.ReadAllText(autoUpdatePath)).Version;
            Path = System.IO.Path.GetDirectoryName(autoUpdatePath);
            IsEnabled = false;
        }

        public string Path { get; private set; }
        public string Version { get; private set; }
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        private const string SETTINGS_GROUP = "DCSInstallationLocations";
        public const string AUTO_UPDATE_CONFIG = "autoupdate.cfg";

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(InstallationLocation), new PropertyMetadata(false, OnChangeEnabled));

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