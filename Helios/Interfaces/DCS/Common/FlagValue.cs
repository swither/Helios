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

using System.Globalization;
using GadrocsWorkshop.Helios.UDPInterface;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public class FlagValue : DCSFunction
    {
        private string _id;
        private string _format;

        private HeliosValue _value;

        public FlagValue(BaseUDPInterface sourceInterface, string id, string device, string name, string description)
            : this(sourceInterface, id, device, name, description, "%0.1f")
        {
        }

        public FlagValue(BaseUDPInterface sourceInterface, string id, string device, string name, string description, string exportFormat)
            : base(sourceInterface, device, name, description)
        {
            _id = id;
            _format = exportFormat;
            DoBuild();
        }

        // deserialization constructor
        public FlagValue(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
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
                SerializedDescription, "", BindingValueUnits.Boolean);
            Values.Add(_value);
            Triggers.Add(_value);
        }

        public override void ProcessNetworkData(string id, string value)
        {
            double parsedValue;
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out parsedValue))
            {
                _value.SetValue(new BindingValue(parsedValue != 0d), false);
            }
        }

        protected override ExportDataElement[] DefaultDataElements => new ExportDataElement[] { new DCSDataElement(_id, _format, true) };

        public override void Reset()
        {
            _value.SetValue(BindingValue.Empty, true);
        }
    }
}