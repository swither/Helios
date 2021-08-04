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
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public class SilentValueConsumer : NetworkFunction
    {
        [JsonProperty("id")]
        protected string _id;

        [JsonProperty("exports", Order = -6, Required = Required.Always)]
        public override ExportDataElement[] DataElements => DefaultDataElements;

        public SilentValueConsumer(BaseUDPInterface sourceInterface, string id, string description)
            : base(sourceInterface)
        {
            _id = id;
            // nothing to build
        }

        // deserialization constructor
        public SilentValueConsumer(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
            : base(sourceInterface)
        {
            // no code
        }

        public override void BuildAfterDeserialization()
        {
            // nothing to do
        }

        public override void ProcessNetworkData(string id, string value)
        {
            // do nothing with it, just avoid logging it as a warning
        }

        // return a data element that does not generate an export
        // but still consumes its value
        protected override ExportDataElement[] DefaultDataElements =>
            new ExportDataElement[] { new DCSDataElement(_id) };

        public override void Reset()
        {
            // no code
        }
    }
}