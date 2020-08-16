//  Copyright 2020 Ammo Goettsch
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

using System;
using System.Xml.Serialization;

namespace GadrocsWorkshop.Helios.Interfaces.Profile
{
    /// <summary>
    /// configuration model for ProfileInterface
    /// </summary>
    [Serializable]
    [XmlRoot("Configuration", Namespace = XML_NAMESPACE)]
    public class ProfileSettings: HeliosXmlModel
    {
        // our schema identifier, in case of future configuration model changes
        public const string XML_NAMESPACE = "http://Helios.local/Base/Interfaces/ProfileInterface/Configuration";

        /// <summary>
        /// backing field for property IgnoreMissingImages, contains
        /// true if the missing image warnings should be suppressed for this profile
        /// </summary>
        private bool _ignoreMissingImages;

        public ProfileSettings() : base(XML_NAMESPACE)
        {
            // no code
        }

        /// <summary>
        /// true if the missing image warnings should be suppressed for this profile
        /// </summary>
        [XmlElement("IgnoreMissingImages")]
        public bool IgnoreMissingImages
        {
            get => _ignoreMissingImages;
            set
            {
                if (_ignoreMissingImages == value) return;
                bool oldValue = _ignoreMissingImages;
                _ignoreMissingImages = value;
                OnPropertyChanged("IgnoreMissingImages", oldValue, value, true);
            }
        }

        public bool IsDefault => !IgnoreMissingImages;
    }
}