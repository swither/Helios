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

using System.Xml.Serialization;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.Interfaces.RTT
{
    [XmlRoot("Network", Namespace = ConfigGenerator.XML_NAMESPACE)]
    public class NetworkOptions : NotificationObject
    {
        private string _ipAddress;
        private string _port;

        #region Properties

        /// <summary>
        /// IPAddress of remote RTT Server
        /// </summary>
        [XmlAttribute("IPAddress")]
        public string IPAddress
        {
            get => _ipAddress;
            set
            {
                if(_ipAddress == value)
                {
                    return;
                }
                string oldValue = _ipAddress;
                _ipAddress = value;
                OnPropertyChanged("IPAddress", oldValue, value, true);
            }
        }

        /// <summary>
        /// Port of remote RTT Server
        /// </summary>
        [XmlAttribute("Port")]
        public string Port
        {
            get => _port;
            set
            {
                if (_port == value)
                {
                    return;
                }
                string oldValue = _port;
                _port = value;
                OnPropertyChanged("Port", oldValue, value, true);
            }
        }
        #endregion
    }
}