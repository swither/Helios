// Copyright 2020 Helios Contributors
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
using System.Xml.Serialization;

namespace GadrocsWorkshop.Helios.HeliosProcessControl
{
    /// <summary>
    /// configuration model for ProcessControlInterface
    /// </summary>
    [Serializable]
    [XmlRoot("Configuration", Namespace = XML_NAMESPACE)]
    public class ProcessControl: HeliosXmlModel
    {
        public const string XML_NAMESPACE = "http://Helios.local/HeliosProcessControl/Interfaces/ProcessControl/Configuration";

        public ProcessControl() : base(XML_NAMESPACE)
        {
            // no code
        }

        // global setting
        [XmlIgnore]
        public bool AllowLaunch
        {
            get => ConfigManager.SettingsManager.LoadSetting("ProcessControl", "AllowLaunch", false);
            set => ConfigManager.SettingsManager.SaveSetting("ProcessControl", "AllowLaunch", value);
        }

        // global setting
        [XmlIgnore]
        public bool AllowKill
        {
            get => ConfigManager.SettingsManager.LoadSetting("ProcessControl", "AllowKill", false);
            set => ConfigManager.SettingsManager.SaveSetting("ProcessControl", "AllowKill", value);
        }
    }
}