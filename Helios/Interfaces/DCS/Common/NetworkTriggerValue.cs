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

using System;
using GadrocsWorkshop.Helios.UDPInterface;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    /// <summary>
    /// a network function that presents as a changeable string value
    ///
    /// NOTE: this function is not serialized to JSON unless "Serializable" is explicitly set
    /// </summary>
    public class NetworkTriggerValue : DCSFunction
    {
        private readonly string _id;
        private HeliosValue _value;
        private HeliosTrigger _receivedTrigger;

        [JsonProperty("valueDescription")]
        private string ValueDescription { get; set; }

        public bool ShouldSerializeValueDescription()
        {
            return !string.IsNullOrEmpty(ValueDescription);
        }

        public class Value : EventArgs
        {
            public string Text { get; set; }
        }

        // fired when a value is received, in addition to firing any Helios bindings
        public event EventHandler<Value> ValueReceived;

        public NetworkTriggerValue(BaseUDPInterface sourceInterface, string id, string name, string description, string valueDescription)
            : base(sourceInterface, "", name, description)
        {
            _id = id;
            ValueDescription = valueDescription;
            Serializable = false;
            DoBuild();
        }

        // deserialization constructor
        public NetworkTriggerValue(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
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
                SerializedDescription, ValueDescription, BindingValueUnits.Text);
            Values.Add(_value);
            Triggers.Add(_value);
            _receivedTrigger = new HeliosTrigger(SourceInterface, SerializedDeviceName, SerializedFunctionName, "received",
                SerializedDescription);
            Triggers.Add(_receivedTrigger);
        }

        // optional additional trigger for received event regardless of whether the data changes
        public HeliosTrigger Received() => _receivedTrigger;

        public override void ProcessNetworkData(string id, string value)
        {
            BindingValue bound = new BindingValue(value);
            _value.SetValue(bound, false);
            _receivedTrigger.FireTrigger(bound);
            ValueReceived?.Invoke(this, new Value {Text = value});
        }

        protected override ExportDataElement[] DefaultDataElements => 
            new ExportDataElement[] { new DCSDataElement(_id) };

        public override void Reset()
        {
            _value.SetValue(BindingValue.Empty, true);
        }
    }
}