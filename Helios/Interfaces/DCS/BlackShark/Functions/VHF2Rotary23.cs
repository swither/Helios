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
    using Common;
    using UDPInterface;
    using System;
    using System.Globalization;

    public class VHF2Rotary23 : DCSFunction
    {
        private HeliosValue _value;

        public VHF2Rotary23(BaseUDPInterface sourceInterface, string id, string device, string name)
            : base(sourceInterface, device, name, null)
        {
            DefaultDataElements = new ExportDataElement[] { new DCSDataElement(id, "%.2f", true) };
            DoBuild();
        }

        // deserialization constructor
        public VHF2Rotary23(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
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
            _value = new HeliosValue(SourceInterface, BindingValue.Empty, SerializedDeviceName, SerializedFunctionName,
                SerializedDescription, "", BindingValueUnits.Numeric);
            Values.Add(_value);
            Triggers.Add(_value);
        }

        public override void ProcessNetworkData(string id, string value)
        {
            if (!double.TryParse(value, NumberStyles.Number,
                CultureInfo.InvariantCulture, out double parseValue))
            {
                return;
            }

            double newValue = Math.Truncate(parseValue * 10d);
            _value.SetValue(new BindingValue(newValue), false);
        }

        protected override ExportDataElement[] DefaultDataElements { get; }

        public override void Reset()
        {
            _value.SetValue(BindingValue.Empty, true);
        }
    }
}
    