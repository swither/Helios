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

using System.Linq;
using GadrocsWorkshop.Helios.UDPInterface;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public abstract class DCSFunction : NetworkFunction
    {
        protected DCSDataElement[] _serializedDataElements = new DCSDataElement[0];

        protected DCSFunction(BaseUDPInterface sourceInterface, string device, string name, string description)
            : base(sourceInterface)
        {
            SerializedDeviceName = device;
            SerializedFunctionName = name;
            SerializedDescription = description;
        }

        // deserialization constructor
        protected DCSFunction(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext _)
            : base(sourceInterface)
        {
            // no code
            // NOTE: we typically do not require the serialization streaming context for anything, but it is
            // required to differentiate the deserialization constructor (which does not Build) from other
            // constructors
        }

        public bool ShouldSerializeSerializedDescription() => !string.IsNullOrEmpty(SerializedDescription);

        #region Overrides

        /// <summary>
        /// access to our data elements, which may be from serialization or from code
        /// </summary>
        [JsonIgnore]
        public override ExportDataElement[] DataElements
        {
            get
            {
                if ((_serializedDataElements.Length == 0) && (DefaultDataElements != null))
                {
                    return DefaultDataElements;
                }

                return _serializedDataElements.Select(element => (ExportDataElement) element).ToArray();
            }
        }

        [JsonIgnore]
        public override string DebugIdentity => $"{SerializedDeviceName} {SerializedFunctionName} {base.DebugIdentity}";

        [JsonIgnore] public override string LocalKey => $"{SerializedDeviceName}.{SerializedFunctionName}";

        #endregion

        #region Properties

        [JsonIgnore]
        public string DeviceName => SerializedDeviceName;

        [JsonIgnore]
        public string Name => SerializedFunctionName;

        [JsonProperty("device", Order = -9, Required = Required.Always)]
        protected string SerializedDeviceName { get; private set; }

        [JsonProperty("name", Order = -8, Required = Required.Always)]
        protected string SerializedFunctionName { get; private set; }

        [JsonProperty("description", Order = -7)]
        protected string SerializedDescription { get; private set; }

        [JsonProperty("exports", Order = -6, Required = Required.Always)]
        protected DCSDataElement[] SerializedDataElements
        {
            get
            {
                ExportDataElement[] raw = DefaultDataElements;
                if (raw == null)
                {
                    // this will happen during deserialization, because the Json deserializer looks at this
                    // value before it is initialized
                    return _serializedDataElements;
                }

                DCSDataElement[] processed = new DCSDataElement[raw.Length];
                for (int i = 0; i < processed.Length; i++)
                {
                    if (raw[i] is DCSDataElement supported)
                    {
                        processed[i] = supported;
                    }
                    else
                    {
                        throw new System.Exception(
                            "implementation error: only DCSDataElement can be serialized to JSON at this time");
                    }
                }

                return processed;
            }
            set => _serializedDataElements = value;
        }

        #endregion
    }
}