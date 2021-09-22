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

    public class SMCFuzeDisplay : DCSFunction
    {
        private HeliosValue _fuzeDisplay;

        public SMCFuzeDisplay(BaseUDPInterface sourceInterface, string deviceId, string id, string device, string name, string description)
            : base(sourceInterface, device, name, description)
        {
            DefaultDataElements = new ExportDataElement[] { new DCSDataElement(id, null, false) };
            DoBuild();
        }

        // deserialization constructor
        public SMCFuzeDisplay(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
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
            _fuzeDisplay = new HeliosValue(SourceInterface, BindingValue.Empty, SerializedDeviceName, SerializedFunctionName,
                SerializedDescription, "", BindingValueUnits.Numeric);
            Values.Add(_fuzeDisplay);
            Triggers.Add(_fuzeDisplay);
        }

        protected override ExportDataElement[] DefaultDataElements { get; }

        public override void ProcessNetworkData(string id, string value)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out double parsedValue))
            {
                _fuzeDisplay.SetValue(new BindingValue(parsedValue), false);
            }
        }

        public override void Reset()
        {
            _fuzeDisplay.SetValue(BindingValue.Empty, true);
        }
    }
}
