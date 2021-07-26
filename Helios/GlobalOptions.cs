// Copyright 2021 Ammo Goettsch
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

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// Global options for Helios Library configured via Tools | Options menu or similar.
    /// Client code may use static accessors to check settings.  UI code will use properties against an instance.
    /// NOTE: the string names of properties in the XML file are intentionally not set from the property names
    /// in case the property names change later.
    /// </summary>
    public class GlobalOptions : NotificationObject
    {
        private const string SETTINGS_GROUP = "Helios";

        /// <summary>
        /// backing field for property ScaleAllText, contains
        /// true if text attached to buttons and similar controls is scaled during reset monitors and similar operations
        /// </summary>
        private bool _scaleAllText;

        public GlobalOptions()
        {
            _scaleAllText = HasScaleAllText;
        }

        #region Properties

        /// <summary>
        /// true if text attached to buttons and similar controls is scaled during reset monitors and similar operations
        /// </summary>
        public bool ScaleAllText
        {
            get => _scaleAllText;
            set
            {
                if (_scaleAllText == value)
                {
                    return;
                }

                bool oldValue = _scaleAllText;
                _scaleAllText = value;
                ConfigManager.SettingsManager.SaveSetting(SETTINGS_GROUP, "ScaleAllText", value);
                OnPropertyChanged("ScaleAllText", oldValue, value, true);
            }
        }

        /// <summary>
        /// accessible as utility for client code that can't get access to the GlobalOptions instance readily
        /// </summary>
        /// <returns>
        /// true if text attached to buttons and similar controls is scaled during reset monitors and similar operations
        /// </returns>
        public static bool HasScaleAllText =>
            ConfigManager.SettingsManager.LoadSetting(SETTINGS_GROUP, "ScaleAllText", true);

        #endregion
    }
}