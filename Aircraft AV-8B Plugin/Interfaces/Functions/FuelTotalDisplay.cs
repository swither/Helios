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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.AV8B.Functions
{
    using GadrocsWorkshop.Helios.Interfaces.DCS.Common;
    using GadrocsWorkshop.Helios.UDPInterface;
    using System.Globalization;

    public class FuelTotalDisplay : DCSFunction
    {
        private static readonly ExportDataElement[] DataElementsTemplate = { new DCSDataElement("2010", null, false) };

        private HeliosValue _fiveDigitDisplay;

        public FuelTotalDisplay(BaseUDPInterface sourceInterface)
            : base(sourceInterface, "Fuel Quantity", "Total display", "Fuel Total value")
        {
            DoBuild();
        }

        // deserialization constructor
        public FuelTotalDisplay(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
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
            _fiveDigitDisplay = new HeliosValue(SourceInterface, BindingValue.Empty, SerializedDeviceName,
                SerializedFunctionName, SerializedDescription, "", BindingValueUnits.Numeric);
            Values.Add(_fiveDigitDisplay);
            Triggers.Add(_fiveDigitDisplay);
        }

        protected override ExportDataElement[] DefaultDataElements => DataElementsTemplate;

        public override void ProcessNetworkData(string id, string value)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out double parsedValue))
            {
                _fiveDigitDisplay.SetValue(new BindingValue(parsedValue), false);
            }
        }

        public override void Reset()
        {
            _fiveDigitDisplay.SetValue(BindingValue.Empty, true);
        }
    }
}
