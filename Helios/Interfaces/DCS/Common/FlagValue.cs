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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    using GadrocsWorkshop.Helios.UDPInterface;
    using System.Globalization;
    using System;

    public class FlagValue : NetworkFunction
    {
        private string _id;
        private string _format;
        private bool _dimmable = false; 

        private HeliosValue _value;
        private HeliosValue _brightness;

        public FlagValue(BaseUDPInterface sourceInterface, string id, string device, string name, string description)
            : this(sourceInterface, id, device, name, description, "%0.1f",false)
        {
        }

        public FlagValue(BaseUDPInterface sourceInterface, string id, string device, string name, string description, bool isDimmable)
            : this(sourceInterface, id, device, name, description, "%0.1f",isDimmable)
        {
        }

        public FlagValue(BaseUDPInterface sourceInterface, string id, string device, string name, string description, string exportFormat, bool isDimmable)
            : base(sourceInterface)
        {
            _id = id;
            _format = exportFormat;
            _dimmable = isDimmable;

            _value = new HeliosValue(sourceInterface, BindingValue.Empty, device, name, description, "", BindingValueUnits.Boolean);
            Values.Add(_value);
            Triggers.Add(_value);
            if (_dimmable)
            {
                _brightness = new HeliosValue(sourceInterface, BindingValue.Empty, device, name + " brightness", description + " brightness percentage", "", BindingValueUnits.Numeric);
                Values.Add(_brightness);
                Triggers.Add(_brightness);
            }
        }

        public override void ProcessNetworkData(string id, string value)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out double parsedValue))
            {
                _value.SetValue(new BindingValue(parsedValue != 0d), false);
                if (_dimmable)
                {
                    _brightness.SetValue(new BindingValue((parsedValue > 1 ? 1d : parsedValue < 0 ? 0d : parsedValue) * 100), false);
                }
            }
        }

        public override ExportDataElement[] GetDataElements()
        {
            return new ExportDataElement[] { new DCSDataElement(_id, _format, true) };
        }

        public override void Reset()
        {
            _value.SetValue(BindingValue.Empty, true);
            if (_dimmable)
            {
                _brightness.SetValue(BindingValue.Empty, true);
            }
        }
    }
}
