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

using System.Text.RegularExpressions;

namespace GadrocsWorkshop.Helios.Util
{
    public static class KnownLinks
    {
        /// <summary>
        /// the root URL of our origin git repo, or null
        /// </summary>
        /// <returns>the URL if we can figure it out, or null</returns>
        public static string GitRepoUrl()
        {
            // download from the current repo, unless overridden by Helios setting without any UI
            string repo = ConfigManager.SettingsManager.LoadSetting("Helios", "Repo", null) ??
                          "https://github.com/HeliosVirtualCockpit/Helios/";

            // don't allow something that isn't a github URL, because otherwise we may do bad things
            // based on URL assumptions
            Match match = new Regex("^https://github.com/[a-zA-Z0-9_]+/[a-zA-Z0-9_]+/").Match(repo);
            return match.Success ? match.Groups[0].Value : null;
        }
    }
}