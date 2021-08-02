// Copyright 2021 Ammo Goettsch
// 
// HeliosFalcon is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// HeliosFalcon is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.Interfaces.RTT
{
    [XmlRoot("ProcessControl", Namespace = ConfigGenerator.XML_NAMESPACE)]
    public class ProcessControl : NotificationObject
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private const string CONFIGURATION_GROUP = "FalconRtt";
        private const string CONFIGURATION_SETTING_ENABLED = "AllowProcessControl";

        /// <summary>
        /// backing field for property StartRTT, contains
        /// true if Helios should start RTT automatically on profile start
        /// </summary>
        private bool _startRtt;

        /// <summary>
        /// backing field for property StopRtt, contains
        /// true of helios should stop RTT automatically on profile stop
        /// </summary>
        private bool _stopRtt;

        /// <summary>
        /// backing field for property Enabled, contains
        /// true if process control operations are allowed on this machine (not per-profile)
        /// </summary>
        private bool _enabled = ConfigManager.SettingsManager.LoadSetting(CONFIGURATION_GROUP, CONFIGURATION_SETTING_ENABLED, false);

        public bool StartRTTClient(string executablePath)
        {
            if (!File.Exists(executablePath))
            {
                return false;
            }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Path.GetFileName(executablePath);
            psi.WorkingDirectory = Path.GetDirectoryName(executablePath);
            psi.UseShellExecute = true;
            psi.RedirectStandardOutput = false;
            Process.Start(psi);
            return true;
        }

        public void KillRTTCllient(string processName)
        {
            try
            {
                Process[] localProcessesByName = Process.GetProcessesByName(processName);
                foreach (Process proc in localProcessesByName)
                {
                    Logger.Info("Killing process image name {ProcessImageName}", processName);
                    proc.Kill();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error caught during kill processing for process image name {ProcessImageName}",
                    processName);
            }
        }

        #region Properties

        /// <summary>
        /// true if Helios should start RTT automatically on profile start
        /// </summary>
        [DefaultValue(false)]
        [XmlElement("StartRtt")]
        public bool StartRtt
        {
            get => _startRtt;
            set
            {
                if (_startRtt == value)
                {
                    return;
                }

                bool oldValue = _startRtt;
                _startRtt = value;
                OnPropertyChanged(nameof(StartRtt), oldValue, value, true);
            }
        }

        /// <summary>
        /// true of helios should stop RTT automatically on profile stop
        /// </summary>
        [DefaultValue(false)]
        [XmlElement("StopRtt")]
        public bool StopRtt
        {
            get => _stopRtt;
            set
            {
                if (_stopRtt == value)
                {
                    return;
                }

                bool oldValue = _stopRtt;
                _stopRtt = value;
                OnPropertyChanged(nameof(StopRtt), oldValue, value, true);
            }
        }

        /// <summary>
        /// true if process control operations are allowed on this machine (not per-profile)
        /// </summary>
        [XmlIgnore]
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                {
                    return;
                }

                bool oldValue = _enabled;
                _enabled = value;
                OnPropertyChanged(nameof(Enabled), oldValue, value, true);
                ConfigManager.SettingsManager.SaveSetting(CONFIGURATION_GROUP, CONFIGURATION_SETTING_ENABLED, value);
            }
        }

        #endregion
    }
}