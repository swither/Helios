//  Copyright 2014 Craig Courtney
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

using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios
{
    public class CalibrationPointDouble : NotificationObject
    {
        private double _input;
        private double _output;

        public CalibrationPointDouble(double input, double outputValue)
        {
            _input = input;
            _output = outputValue;
        }

        [JsonProperty("value")]
        public double Value
        {
            get
            {
                return _input;
            }
            set
            {
                if (!_input.Equals(value))
                {
                    double oldValue = _input;
                    _input = value;
                    OnPropertyChanged("Value", oldValue, value, true);
                }
            }
        }

        /// <summary>
        /// WARNING: this is not actually a multiplier, it is the mapped output value but it is named wrong
        /// </summary>
        [JsonProperty("mappedValue")]
        public double Multiplier
        {
            get
            {
                return _output;
            }
            set
            {
                double oldValue = _output;
                _output = value;
                OnPropertyChanged("Value", oldValue, value, true);
            }
        }

        [JsonIgnore]
        public double MappedValue => Multiplier; // name correction for code readability
    }
}
