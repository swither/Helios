// Copyright 2020 Ammo Goettsch
// 
// Patching is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Patching is distributed in the hope that it will be useful,
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
using GadrocsWorkshop.Helios.Util.Shadow;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public class DCSMonitorEventArgs : EventArgs
    {
        public DCSMonitorEventArgs(DCSMonitor shadow)
        {
            Data = shadow;
        }

        #region Properties

        public DCSMonitor Data { get; }

        #endregion
    }

    /// <summary>
    /// This class represents a Helios monitor being observed for use with DCS.
    /// It is the model class.
    /// </summary>
    [DebuggerDisplay("Shadow for {" + nameof(Monitor) + "}")]
    public class DCSMonitor : ShadowMonitorBase<DCSMonitorEventArgs>
    {
        public event EventHandler<KeyChangeEventArgs> KeyChanged;

        /// <summary>
        /// backing field for property Included, contains
        /// true if this monitor is included in the extent used to calculate the DCS resolution
        /// </summary>
        private bool _included;

        /// <summary>
        /// backing field for property Main, contains
        /// true if this monitor is included in Main 3D view
        /// </summary>
        private bool _main;

        /// <summary>
        /// backing field for property UserInterface, contains
        /// true if this monitor is included in UI view showing DCS dialogs and loading screen
        /// </summary>
        private bool _userInterface;

        internal DCSMonitor(IShadowVisualParent parent, Monitor monitor)
            : base(parent, monitor, monitor, false)
        {
            Key = CreateKey(monitor);

            // read settings for monitors matching this geometry
            LoadSettings();
        }

        // the position and size of a shadow is all we care about
        public static string CreateKey(Monitor display) =>
            $"{display.Left}_{display.Top}_{display.Width}_{display.Height}";

        /// <summary>
        /// operations that are currently valid on this monitor based on layout mode
        /// </summary>
        [Flags]
        public enum PermissionsFlags
        {
            None = 0,
            CanInclude = 1,
            CanExclude = 2
        }

        // this key represents the entire configuration state for this monitor when used for DCS
        public string CreateStateKey()
        {
            string main = Main ? " MAIN" : "";
            string ui = UserInterface ? " UI" : "";
            return
                $"{Monitor.Left} {Monitor.Top} {Monitor.Width} {Monitor.Height}{main}{ui}";
        }

        internal static IEnumerable<string> GetAllKeys(Monitor monitor)
        {
            string baseKey = CreateKey(monitor);
            yield return baseKey;
            yield return $"{baseKey}_Main";
            yield return $"{baseKey}_UserInterface";
        }

        internal void Lockout()
        {
            UserInterface = false;
            Main = false;
            Included = false;
            Permissions = PermissionsFlags.CanExclude;
        }

        private void LoadSettings()
        {
            _included = ConfigManager.SettingsManager.LoadSetting(MonitorSetup.DISPLAYS_SETTINGS_GROUP, Key, true);
            _main = ConfigManager.SettingsManager.LoadSetting(MonitorSetup.DISPLAYS_SETTINGS_GROUP, $"{Key}_Main",
                false);
            _userInterface = ConfigManager.SettingsManager.LoadSetting(MonitorSetup.DISPLAYS_SETTINGS_GROUP,
                $"{Key}_UserInterface",
                false);
        }

        #region Overrides

        public override void Instrument()
        {
            Instrument(Monitor, Monitor);
        }

        protected override void OnVisualModified()
        {
            base.OnVisualModified();

            // the monitor we shadow may have changed enough to where our key
            // doesn't match (during reset monitors)
            // and so we need to reindex in that case and load settings again
            string newKey = CreateKey(Monitor);
            if (newKey == Key)
            {
                return;
            }

            string oldKey = Key;
            Key = newKey;
            LoadSettings();
            KeyChanged?.Invoke(this, new KeyChangeEventArgs(oldKey, newKey));
        }

        public override bool AddViewport()
        {
            ViewportCount++;
            return ViewportCount == 1;
        }

        public override bool RemoveViewport()
        {
            ViewportCount--;
            return ViewportCount == 0;
        }

        public override DCSMonitorEventArgs CreateEvent() => new DCSMonitorEventArgs(this);

        #endregion

        #region Properties

        public string Key { get; private set; }

        /// <summary>
        /// true if this monitor is included in Main 3D view
        /// </summary>
        public bool Main
        {
            get => _main;
            set
            {
                if (_main == value)
                {
                    return;
                }

                bool oldValue = _main;
                _main = value;
                if (ConfigManager.Application.SettingsAreWritable)
                {
                    ConfigManager.SettingsManager.SaveSetting(MonitorSetup.DISPLAYS_SETTINGS_GROUP, $"{Key}_Main",
                        value);
                }
                else
                {
                    ConfigManager.LogManager.LogWarning(
                        $"unable to persist unexpected monitor setup override that does not match current settings: {Key} Main set to {value}");
                }

                OnPropertyChanged("Main", oldValue, value, true);
            }
        }

        /// <summary>
        /// true if this monitor is included in UI view showing DCS dialogs and loading screen
        /// </summary>
        public bool UserInterface
        {
            get => _userInterface;
            set
            {
                if (_userInterface == value)
                {
                    return;
                }

                bool oldValue = _userInterface;
                _userInterface = value;
                if (ConfigManager.Application.SettingsAreWritable)
                {
                    ConfigManager.SettingsManager.SaveSetting(MonitorSetup.DISPLAYS_SETTINGS_GROUP,
                        $"{Key}_UserInterface", value);
                }
                else
                {
                    ConfigManager.LogManager.LogWarning(
                        $"unable to persist unexpected monitor setup override that does not match current settings: {Key} UserInterface set to {value}");
                }

                OnPropertyChanged("UserInterface", oldValue, value, true);
            }
        }

        /// <summary>
        /// true if this monitor is included in the extent used to calculate the DCS resolution
        /// </summary>
        public bool Included
        {
            get => _included;
            set
            {
                if (_included == value)
                {
                    return;
                }

                bool oldValue = _included;
                _included = value;
                if (ConfigManager.Application.SettingsAreWritable)
                {
                    ConfigManager.SettingsManager.SaveSetting(MonitorSetup.DISPLAYS_SETTINGS_GROUP, Key, value);
                }
                else
                {
                    ConfigManager.LogManager.LogWarning(
                        $"unable to persist unexpected monitor setup override that does not match current settings: {Key} Included set to {value}");
                }

                OnPropertyChanged("Included", oldValue, value, true);
            }
        }

        /// <summary>
        /// allowable operations on this monitor, based on layout mode and geometry
        /// </summary>
        public PermissionsFlags Permissions { get; internal set; }

        public bool HasContent => Main || UserInterface || ViewportCount > 0;

        internal int ViewportCount { get; private set; }

        #endregion

        public class KeyChangeEventArgs : EventArgs
        {
            public KeyChangeEventArgs(string oldKey, string newKey)
            {
                OldKey = oldKey;
                NewKey = newKey;
            }

            #region Properties

            public string OldKey { get; }
            public string NewKey { get; }

            #endregion
        }
    }
}