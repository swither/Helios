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

using GadrocsWorkshop.Helios.UDPInterface;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public abstract class DCSFunctionPair: DCSFunction
    {
        protected DCSFunctionPair(
            BaseUDPInterface sourceInterface, 
            string device, string name, string description,
            string device2, string name2, string description2) 
            : base(sourceInterface, device, name, description)
        {
            SerializedDeviceName2 = device2;
            SerializedFunctionName2 = name2;
            SerializedDescription2 = description2;
        }

        // deserialization constructor
        public DCSFunctionPair(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
            : base(sourceInterface, context)
        {
            // no code
        }

        #region JsonOnly
        [JsonProperty("device2")]
        protected string SerializedDeviceName2 { get; private set; }
        [JsonProperty("name2")]
        protected string SerializedFunctionName2 { get; private set; }
        [JsonProperty("description2")]
        protected string SerializedDescription2 { get; private set; }

        public bool ShouldSerializeSerializedDescription2()
        {
            return (SerializedDescription2 != null) && (SerializedDescription2.Length > 0);
        }
        #endregion
    }
}
