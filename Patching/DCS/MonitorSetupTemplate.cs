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

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    /// <summary>
    /// specific naming and handling that differs between a shared monitor setup file and a separate monitor setup file
    /// </summary>
    internal class MonitorSetupTemplate
    {
        public MonitorSetupTemplate(string combinedMonitorSetupBaseName, string profileName, bool combined)
        {
            ProfileName = profileName;
            Combined = combined;
            ProfileBaseName = ProfileName?.Replace(" ", "") ?? "";
            CombinedMonitorSetupBaseName = combinedMonitorSetupBaseName;
        }

        /// <summary>
        /// name of the current profile
        /// </summary>
        public string ProfileName { get; }

        /// <summary>
        /// base name of the separate monitor setup file if we were to generate one for the current profile
        /// </summary>
        public string ProfileBaseName { get; }

        /// <summary>
        /// base name of combined monitor setup file
        /// </summary>
        public string CombinedMonitorSetupBaseName { get; }

        /// <summary>
        /// true if this template is generating a combined setup
        /// </summary>
        public bool Combined { get; }

        /// <summary>
        /// effective base name of generated lua file, without path or extension
        /// </summary>
        public string MonitorSetupFileBaseName => Combined ? CombinedMonitorSetupBaseName : $"H_{ProfileBaseName}";

        /// <summary>
        /// the name that the monitor setup file will report to DCS for selection in the UI
        /// 
        /// NOTE: for now, the internal name of the monitor setup is always the same as the file name
        /// </summary>
        public string MonitorSetupName => MonitorSetupFileBaseName;

        /// <summary>
        /// a comment placed in the monitor setup Lua file
        /// </summary>
        public string SourcesList =>
            Combined ? "compatible Helios Profiles" : $"Helios Profile {ProfileName ?? "<unsaved>"}";

        /// <summary>
        /// effective file name of the monitor setup lua file
        /// </summary>
        public string FileName => $"{MonitorSetupFileBaseName}.lua";
    }
}