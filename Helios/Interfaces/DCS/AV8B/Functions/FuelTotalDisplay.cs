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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.AV8B.Functions
{
    using GadrocsWorkshop.Helios.Interfaces.DCS.Common;
    using GadrocsWorkshop.Helios.UDPInterface;
    using System;
    using System.Globalization;

    public class FuelTotalDisplay : NetworkFunction
    {
        private static DCSDataElement[] _dataElements = new DCSDataElement[] { new DCSDataElement("2010", null, false) };

        private HeliosValue _five_digit_display;

        public FuelTotalDisplay(BaseUDPInterface sourceInterface)
            : base(sourceInterface)
        {
            _five_digit_display = new HeliosValue(sourceInterface, BindingValue.Empty, "Fuel Quantity", "Total display", "Fuel Total value", "", BindingValueUnits.Numeric);
            Values.Add(_five_digit_display);
            Triggers.Add(_five_digit_display);
        }

        public override ExportDataElement[] GetDataElements()
        {
            return _dataElements;
        }

        public override void ProcessNetworkData(string id, string value)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out double parsedValue))
            {
                _five_digit_display.SetValue(new BindingValue(parsedValue), false);
            }
        }

        public override void Reset()
        {
            _five_digit_display.SetValue(BindingValue.Empty, true);
        }
    }
}
