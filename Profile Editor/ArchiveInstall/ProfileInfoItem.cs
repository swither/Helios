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

using System;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.ProfileEditor.ArchiveInstall
{
    public class ProfileInfoItem
    {
        [JsonProperty("ProfileName", NullValueHandling = NullValueHandling.Ignore)]
        public string ProfileName { get; internal set; }

        [JsonProperty("ReleaseDate", NullValueHandling = NullValueHandling.Ignore)]
        public string ReleaseDate { get; internal set; }

        [JsonProperty("Description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; internal set; }

        [JsonProperty("Version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; internal set; }

        [JsonProperty("License", NullValueHandling = NullValueHandling.Ignore)]
        public string License { get; internal set; }

        [JsonProperty("Aircraft", NullValueHandling = NullValueHandling.Ignore)]
        public string Aircraft { get; internal set; }

        [JsonProperty("Repository", NullValueHandling = NullValueHandling.Ignore)]
        public string Repository { get; internal set; }

        [JsonProperty("Requirements", NullValueHandling = NullValueHandling.Ignore)]
        public string Requirements { get; internal set; }

        [JsonProperty("NativeResolution", NullValueHandling = NullValueHandling.Ignore)]
        public string NativeResolution { get; internal set; }

        [JsonProperty("AspectRatio", NullValueHandling = NullValueHandling.Ignore)]
        public string AspectRatio { get; internal set; }
    }
}