// Copyright 2023 Helios Contributors
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

using System.ComponentModel;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public enum IrisConfigurationType
    {
        [Description("Iris Screen Exporter is not used with this profile")]
        NoIris,

        [Description("The current profile contains viewports for the computer running DCS")]
        IrisServerConfig,

        [Description("The current Profile will be run on the remote computer running Helios")]
        IrisClientConfig
    }
}