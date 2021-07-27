//  Copyright 2014 Craig Courtney
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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.BlackShark.Functions
{
    using GadrocsWorkshop.Helios.Interfaces.DCS.Common;
    using GadrocsWorkshop.Helios.UDPInterface;
    using System;
    using System.Globalization;

    public class HSIRange : DCSFunction
    {
        private static readonly ExportDataElement[] DataElementsTemplate = { new DCSDataElement("117", "%0.4f", true), new DCSDataElement("527", "%0.4f", true), new DCSDataElement("528", "%0.4f", true) };

        private double _hundreds;
        private double _tens;
        private double _units;

        private HeliosValue _range;

        public HSIRange(BaseUDPInterface sourceInterface)
            : base(sourceInterface, "HSI", "Range to bearing", "Range in kilometers to current beacon.")
        {
            DoBuild();
        }

        // deserialization constructor
        public HSIRange(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
            : base(sourceInterface, context)
        {
            // no code
        }

        public override void BuildAfterDeserialization()
        {
            DoBuild();
        }

        private void DoBuild()
        {
            _range = new HeliosValue(SourceInterface, BindingValue.Empty, SerializedDeviceName, SerializedFunctionName,
                SerializedDescription, "", BindingValueUnits.Kilometers);
            Values.Add(_range);
            Triggers.Add(_range);
        }

        protected override ExportDataElement[] DefaultDataElements => DataElementsTemplate;

        public override void ProcessNetworkData(string id, string value)
        {
            switch (id)
            {
                case "117":
                    _hundreds = ClampedParse(value, 100d);
                    break;
                case "527":
                    _tens = ClampedParse(value, 10d);
                    break;
                case "528":
                    _units = Parse(value, 1d);
                    break;
            }
            double distance = _hundreds + _tens + _units;
            _range.SetValue(new BindingValue(distance), false);
        }

        private double Parse(string value, double scale)
        {
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat,
                out double scaledValue))
            {
                return scaledValue;
            }

            if (scaledValue < 1.0d)
            {
                scaledValue *= scale * 10d;
            }
            else
            {
                scaledValue = 0d;
            }
            return scaledValue;
        }

        private double ClampedParse(string value, double scale)
        {
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat,
                out double scaledValue))
            {
                return scaledValue;
            }

            if (scaledValue < 1.0d)
            {
                scaledValue = Math.Truncate(scaledValue * 10d) * scale;
            }
            else
            {
                scaledValue = 0d;
            }
            return scaledValue;
        }

        public override void Reset()
        {
            _range.SetValue(BindingValue.Empty, true);
        }
    }
}
