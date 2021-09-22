// Copyright 2020 Helios Contributors
// 
// Helios is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Helios is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using GadrocsWorkshop.Helios.UDPInterface;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public class Text : DCSFunction
    {
        private string _id;

        private HeliosValue _value;

        public Text(BaseUDPInterface sourceInterface, string id, string device, string name, string description)
            : base(sourceInterface, device, name, description)
        {
            _id = id;
            DoBuild();
        }

        // deserialization constructor
        public Text(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
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
                SerializedDescription, "", BindingValueUnits.Text);
            Values.Add(_value);
            Triggers.Add(_value);
        }

        public override void ProcessNetworkData(string id, string value)
        {
            _value.SetValue(new BindingValue(value), false);
        }

        protected override ExportDataElement[] DefaultDataElements => new ExportDataElement[] { new DCSDataElement(_id) };
       
        public override void Reset()
        {
            _value.SetValue(BindingValue.Empty, true);
        }

    }
}
