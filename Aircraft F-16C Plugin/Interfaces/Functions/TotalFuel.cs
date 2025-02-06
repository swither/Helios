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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.F16C.Functions
{
    using GadrocsWorkshop.Helios.Interfaces.DCS.Common;
    using GadrocsWorkshop.Helios.UDPInterface;
    using GadrocsWorkshop.Helios.Util;
    using System;
    using System.Globalization;

    public class TotalFuel : DCSFunction
    {
        /// TODO: Use this in an interface
#pragma warning disable CS0649
        private static ExportDataElement[] DataElementsTemplate;
#pragma warning restore CS0649

        private HeliosValue _fuel;

        public TotalFuel(BaseUDPInterface sourceInterface) : this(sourceInterface, "2090", "Fuel System", "Total Fuel", "Fuel amount shown on the totalizer.") { }    
        public TotalFuel(BaseUDPInterface sourceInterface, string arg, string device, string elementName, string description)
            : base(sourceInterface, device, elementName, description)
        {
            DataElementsTemplate[0] = new DCSDataElement(arg, null, true);
            DoBuild();
        }

        // deserialization constructor
        public TotalFuel(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
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
            _fuel = new HeliosValue(SourceInterface, BindingValue.Empty, SerializedDeviceName, SerializedFunctionName,
                SerializedDescription, "", BindingValueUnits.Pounds);
            Values.Add(_fuel);
            Triggers.Add(_fuel);
        }

        protected override ExportDataElement[] DefaultDataElements => DataElementsTemplate;

        public override void ProcessNetworkData(string id, string value)
        {
            string[] parts = Tokenizer.TokenizeAtLeast(value, 3, ';');

            double tenThousands = ClampedParse(parts[0], 10000d);
            double thousands = ClampedParse(parts[1], 1000d);
            double hundreds = Parse(parts[2], 100d);

            double fuel = tenThousands + thousands + hundreds;
            _fuel.SetValue(new BindingValue(fuel), false);
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
            _fuel.SetValue(BindingValue.Empty, true);
        }
    }
}
