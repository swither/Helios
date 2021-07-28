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
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.ProfileEditor.ArchiveInstall
{
    public class Option
    {
        /// <summary>
        /// primary unstructured description about this option
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// optional additional structured information about this option
        /// </summary>
        [JsonProperty("info", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<StructuredInfo> Info { get; internal set; }

        [JsonProperty("excludes")]
        public IEnumerable<string> PathExclusions { get; internal set; }

        [JsonProperty("versionsRequired", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<VersionRequired> VersionsRequired { get; internal set; }

        /// <summary>
        /// true if this option may be selected
        /// </summary>
        [JsonIgnore]
        public bool IsValid { get; internal set; } = true;

        /// <summary>
        /// if IsValid is false, this may contain a helpful message about it
        /// </summary>
        [JsonIgnore]
        public string ValidityNarrative { get; internal set; }
    }
}