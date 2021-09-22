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
    public class ProfileManifest16
    {
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)] 
        public string Description { get; internal set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; internal set; }

        [JsonProperty("authors", NullValueHandling = NullValueHandling.Include)]
        public IEnumerable<string> Authors { get; internal set; }

        [JsonProperty("license", NullValueHandling = NullValueHandling.Ignore)]
        public string License { get; internal set; }

        /// <summary>
        /// optional additional structured information about this archive's contents
        /// </summary>
        [JsonProperty("info", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<StructuredInfo> Info { get; internal set; }

        [JsonProperty("versionsRequired", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<VersionRequired> VersionsRequired { get; internal set; }

        [JsonProperty("choices")]
        public IEnumerable<Choice> Choices { get; internal set; }
    }
}