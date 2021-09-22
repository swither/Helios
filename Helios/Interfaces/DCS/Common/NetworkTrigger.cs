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

using GadrocsWorkshop.Helios.UDPInterface;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    /// <summary>
    /// A network function that presents as an event that is fired when a message is received.  Any
    /// attached value is only available in the event.
    ///
    /// NOTE: this function is not serialized to JSON unless "Serializable" is explicitly set
    /// </summary>
    public class NetworkTrigger : DCSFunction
    {
        private readonly string _id;
        private HeliosTrigger _receivedTrigger;

        // without a received value, this presents as an event without value
        public NetworkTrigger(BaseUDPInterface sourceInterface, string id, string name, string description)
            : base(sourceInterface, "", name, description)
        {
            _id = id;
            Serializable = false;
            DoBuild();
        }

        // deserialization constructor
        public NetworkTrigger(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
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
            _receivedTrigger = new HeliosTrigger(SourceInterface, SerializedDeviceName, SerializedFunctionName, "received",
                SerializedDescription);
            Triggers.Add(_receivedTrigger);
        }

        public override void ProcessNetworkData(string id, string value)
        {
            BindingValue bound = new BindingValue(value);
            _receivedTrigger.FireTrigger(bound);
        }

        protected override ExportDataElement[] DefaultDataElements =>
            new ExportDataElement[] { new DCSDataElement(_id) };

        public override void Reset()
        {
        }
    }
}