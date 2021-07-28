//  Copyright 2014 Craig Courtney
//  Copyright 2020 Helios Contributors
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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    using GadrocsWorkshop.Helios.UDPInterface;
    using Newtonsoft.Json;

    public class DCSDataElement : ExportDataElement
    {
        /// <summary>
        /// This constructor created an element that consumes a value but does not generate an export
        /// </summary>
        /// <param name="id"></param>
        public DCSDataElement(string id)
            : this(id, null, false)
        {
            // utility constructor
        }

        /// <summary>
        /// This constructor creates an element that consumes a value and generates an export with the given format
        /// </summary>
        /// <param name="id"></param>
        /// <param name="format"></param>
        public DCSDataElement(string id, string format)
            : this (id, format, false)
        {
            // utility constructor
        }

        public DCSDataElement(string id, string format, bool everyFrame)
            : base(id)
        {
            Format = format;
            IsExportedEveryFrame = everyFrame;
        }

        // deserialization constructor called by reflection from JSON library
        [JsonConstructor]
        private DCSDataElement()
        {
            // default is true if not mentioned in JSON
            IsExportedEveryFrame = true;
        }

        [JsonProperty("format", NullValueHandling = NullValueHandling.Ignore)]
        public string Format { get; private set; }

        [JsonProperty("isExportedEveryFrame")]
        public bool IsExportedEveryFrame { get; private set; }

        #region JsonOnly
        public bool ShouldSerializeIsExportedEveryFrame()
        {
            return !IsExportedEveryFrame;
        }
        #endregion
    }
}
