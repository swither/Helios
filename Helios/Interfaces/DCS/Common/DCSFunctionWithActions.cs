// Copyright 2020 Ammo Goettsch
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

using System.Collections.Generic;
using GadrocsWorkshop.Helios.UDPInterface;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public abstract class DCSFunctionWithActions : DCSFunction
    {
        public class SerializedAction
        {
            [JsonProperty("deviceId", Required = Required.Always)]
            public string DeviceID { get; set; }

            [JsonProperty("actionId", Required = Required.Always)]
            public string ActionID { get; set; }

            // can be null when used with an axis or other function that calculates its argument value
            [JsonProperty("actionValue", NullValueHandling=NullValueHandling.Ignore)]
            public string ActionValue { get; set; }

            // calculated protocol value
            [JsonIgnore]
            public string CommandString => $"C{DeviceID},{ActionID},{ActionValue}";
        }

        [JsonProperty("actions", Required = Required.Always)]
        protected Dictionary<string, SerializedAction> SerializedActions { get; private set; } = new Dictionary<string, SerializedAction>();

        protected DCSFunctionWithActions(BaseUDPInterface sourceInterface, string device, string name, string description) 
            : base(sourceInterface, device, name, description)
        {
        }

        // deserialization constructor
        protected DCSFunctionWithActions(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
            : base(sourceInterface, context)
        {
            // no code
        }

    }
}
