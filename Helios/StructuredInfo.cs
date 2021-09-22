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

using System.Reflection.Emit;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// structured information, that can be displayed in the UI or otherwise formatted
    /// </summary>
    public class StructuredInfo
    {
        // deserialization constructor
        public StructuredInfo()
        {
            // no code
        }

        // utility constructor
        public StructuredInfo(string label, string value)
        {
            Label = label;
            Value = value;
        }

        /// <summary>
        /// a label or property name that identifies this piece of information
        ///
        /// can be displayed as a label
        /// </summary>
        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label
        {
            get;
            set;
        }

        /// <summary>
        /// view model helper
        /// </summary>
        [JsonIgnore] 
        public string LabelWithColon => string.IsNullOrEmpty(Label) ? Label : $"{Label}:";

        /// <summary>
        /// a value that might in the future be used to drive some behavior
        ///
        /// requires Label
        /// can be displayed as a short value
        /// </summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value
        {
            get;
            set;
        }

        /// <summary>
        /// a user-readable message
        ///
        /// associated with Label (if any)
        /// can be displayed as text
        /// </summary>
        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message
        {
            get;
            set;
        }
    }
}
