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
        [JsonProperty("Description", NullValueHandling = NullValueHandling.Ignore)] 
        public string Description { get; internal set; }

        [JsonProperty("Version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; internal set; }

        [JsonProperty("Authors", NullValueHandling = NullValueHandling.Include)]
        public IEnumerable<string> Authors { get; internal set; }

        [JsonProperty("License", NullValueHandling = NullValueHandling.Ignore)]
        public string License { get; internal set; }

        [JsonProperty("ProfileInfo", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<ProfileInfoItem> ProfileInfoItems { get; internal set; }

        [JsonProperty("VersionsRequired", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<VersionRequired> VersionsRequired { get; internal set; }

        [JsonProperty("Choices")]
        public IEnumerable<Choice> Choices { get; internal set; }
    }
}