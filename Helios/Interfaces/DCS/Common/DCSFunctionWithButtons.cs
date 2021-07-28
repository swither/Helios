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
    public abstract class DCSFunctionWithButtons : DCSFunction
    {
        protected class SerializedButton
        {
            [JsonProperty("deviceId", Required = Required.Always)]
            public string DeviceID { get; set; }

            [JsonProperty("pushId", Required = Required.Always)]
            public string PushID { get; set; }

            [JsonProperty("pushValue", Required = Required.DisallowNull)]
            public string PushValue { get; set; } = "1.0";

            [JsonProperty("releaseId")]
            public string ReleaseID { get; set; }

            [JsonProperty("releaseValue")]
            public string ReleaseValue { get; set; }
        }

        [JsonProperty("buttons")]
        protected List<SerializedButton> SerializedButtons { get; private set; } = new List<SerializedButton>();

        protected DCSFunctionWithButtons(BaseUDPInterface sourceInterface, string device, string name, string description) 
            : base(sourceInterface, device, name, description)
        {
        }

        // deserialization constructor
        public DCSFunctionWithButtons(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
            : base(sourceInterface, context)
        {
            // no code
        }
    }
}
