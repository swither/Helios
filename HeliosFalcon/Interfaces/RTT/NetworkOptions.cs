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
        /// <summary>
        /// backing field for remote ip address, contains
        /// ip address of remote RTT server
        /// </summary>
        private string _ipAddress;

        /// <summary>
        /// backing field for remote port, contains
        /// port of remote RTT server
        /// </summary>
        private string _port;

        /// <summary>
        /// backing field for DataF4 options, contains
        /// true if you wish to enable remote sharedmemory support for F4 Data
        /// </summary>
        private bool _dataF4;

        /// <summary>
        /// backing field for DataBms options, contains
        /// true if you wish to enable remote sharedmemory support for BMS Data
        /// </summary>
        private bool _dataBms;

        /// <summary>
        /// backing field for DataOsb options, contains
        /// true if you wish to enable remote sharedmemory support for OSB Data
        /// </summary>
        private bool _dataOsb;

        /// <summary>
        /// backing field for DataIvibe options, contains
        /// true if you wish to enable remote sharedmemory support for IVibe Data
        /// </summary>
        private bool _dataIvibe;

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

        /// <summary>
        /// DATA_F4 switch
        /// </summary>
        [XmlAttribute("DataF4")]
        public bool DataF4
        {
            get => _dataF4;
            set
            {
                if(_dataF4 == value)
                {
                    return;
                }
                bool oldValue = _dataF4;
                _dataF4 = value;
                OnPropertyChanged(nameof(_dataF4), oldValue, value, true);
            }
        }

        /// <summary>
        /// DATA_BMS switch
        /// </summary>
        [XmlAttribute("DataBms")]
        public bool DataBms
        {
            get => _dataBms;
            set
            {
                if (_dataBms == value)
                {
                    return;
                }
                bool oldValue = _dataBms;
                _dataBms = value;
                OnPropertyChanged(nameof(_dataBms), oldValue, value, true);
            }
        }

        /// <summary>
        /// DATA_OSB switch
        /// </summary>
        [XmlAttribute("DataOsb")]
        public bool DataOsb
        {
            get => _dataOsb;
            set
            {
                if (_dataOsb == value)
                {
                    return;
                }
                bool oldValue = _dataOsb;
                _dataOsb = value;
                OnPropertyChanged(nameof(_dataOsb), oldValue, value, true);
            }
        }

        /// <summary>
        /// DATA_IVIBE switch
        /// </summary>
        [XmlAttribute("DataIvibe")]
        public bool DataIvibe
        {
            get => _dataIvibe;
            set
            {
                if (_dataIvibe == value)
                {
                    return;
                }
                bool oldValue = _dataIvibe;
                _dataIvibe = value;
                OnPropertyChanged(nameof(_dataIvibe), oldValue, value, true);
            }
        }
        #endregion
    }
}