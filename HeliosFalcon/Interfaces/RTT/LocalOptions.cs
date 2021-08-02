// Copyright 2021 Helios Contributors
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

using System.ComponentModel;
using System.Xml.Serialization;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.Interfaces.RTT
{
    [XmlRoot("Local", Namespace = ConfigGenerator.XML_NAMESPACE)]
    public class LocalOptions : NotificationObject
    {
        public const int DEFAULT_FPS = 30;

        /// <summary>
        /// backing field for property FramesPerSecond, contains
        /// RTT updates per second
        /// </summary>
        private int _framesPerSecond = DEFAULT_FPS;

        /// <summary>
        /// backing field for property RwrGrid, contains
        /// true if RWR Grid functionality is enabled
        /// </summary>
        private bool _rwrGrid;

        #region Properties

        /// <summary>
        /// RTT updates per second
        /// </summary>
        [XmlElement("FramesPerSecond")]
        [DefaultValue(DEFAULT_FPS)]
        public int FramesPerSecond
        {
            get => _framesPerSecond;
            set
            {
                if (_framesPerSecond == value)
                {
                    return;
                }

                int oldValue = _framesPerSecond;
                _framesPerSecond = value;
                OnPropertyChanged(nameof(FramesPerSecond), oldValue, value, true);
            }
        }

        /// <summary>
        /// RWR GRID enabled
        /// </summary>
        [DefaultValue(false)]
        [XmlElement("RWRGrid")]
        public bool RWRGrid
        {
            get => _rwrGrid;
            set
            {
                if (_rwrGrid == value)
                {
                    return;
                }

                bool oldValue = _rwrGrid;
                _rwrGrid = value;
                OnPropertyChanged(nameof(RWRGrid), oldValue, value, true);
            }
        }

        #endregion
    }
}