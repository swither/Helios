using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    internal class CombinedMonitorSetup
    {
        private static readonly string VIEWPORT_SETUPS_FOLDER = "Viewport Setups";
        private static readonly string GROUP_NAME = "DCSMonitorSetupCombined";

        internal ICollection<string> KnownViewportSetupNames = new HashSet<string>();

        internal void Initialize()
        {
            // any names in the settings are known, even if their files have been lost
            if (ConfigManager.SettingsManager is ISettingsManager2 settings)
            {
                foreach (string name in settings.EnumerateSettingNames(GROUP_NAME))
                {
                    KnownViewportSetupNames.Add(name);
                }
            }

            // create or scan the storage folder
            string folder = Path.Combine(ConfigManager.DocumentPath, VIEWPORT_SETUPS_FOLDER);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                return;
            }
            foreach (string path in Directory.EnumerateFiles(folder, "*.hvpf.json"))
            {
                KnownViewportSetupNames.Add(Path.GetFileName(path).Replace(".hvpf.json", ""));
            }
        }

        internal ViewportSetupFile Load(string viewportSetupName)
        {
            if (null == viewportSetupName)
            {
                return null;
            }
            string path = LocateFile(viewportSetupName);
            if (!File.Exists(path))
            {
                return null;
            }
            string content = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ViewportSetupFile>(content);
        }

        internal void Save(string viewportSetupName, ViewportSetupFile data)
        {
            string path = LocateFile(viewportSetupName);
            string contents = JsonConvert.SerializeObject(data);
            File.WriteAllText(path, contents);
        }

        internal void Delete(string viewportSetupName)
        {
            if (null == viewportSetupName)
            {
                return;
            }
            string path = LocateFile(viewportSetupName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public bool IsCombined(string viewportSetupName)
        {
            return null != viewportSetupName && ConfigManager.SettingsManager.LoadSetting(GROUP_NAME, viewportSetupName, false);
        }

        internal void SetCombined(string viewportSetupName)
        {
            if (null == viewportSetupName)
            {
                return;
            }
            ConfigManager.SettingsManager.SaveSetting(GROUP_NAME, viewportSetupName, true);
        }

        internal void SetExcluded(string viewportSetupName)
        {
            if (null == viewportSetupName)
            {
                return;
            }
            if (ConfigManager.SettingsManager is ISettingsManager2 settings)
            {
                settings.DeleteSetting(GROUP_NAME, viewportSetupName);
            }
            else
            {
                ConfigManager.SettingsManager.SaveSetting(GROUP_NAME, viewportSetupName, false);
            }
        }

        private string LocateFile(string viewportSetupName)
        {
            Debug.Assert(null != viewportSetupName);
            return Path.Combine(ConfigManager.DocumentPath, VIEWPORT_SETUPS_FOLDER, $"{viewportSetupName}.hvpf.json");
        }

        internal IEnumerable<string> CalculateCombinedSetupNames()
        {
            if (ConfigManager.SettingsManager is ISettingsManager2 settings)
            {
                return settings.EnumerateSettingNames(GROUP_NAME);
            }
            return new string[0];
        }
    }
}
