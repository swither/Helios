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
// 

namespace GadrocsWorkshop.Helios.Interfaces.DCS.MIRAGEF1.EE
{
    using ComponentModel;
    using Common;

    /// <summary>
    /// Interface for DCS Mirage F1EE, including devices which are unique to this variant.
    /// </summary>
    [HeliosInterface("Helios.MIRAGEF1EE", "DCS Mirage F1EE", typeof(DCSInterfaceEditor), typeof(UniqueHeliosInterfaceFactory), UniquenessKey = "Helios.DCSInterface")]
    public class MirageF1CEInterface : MirageF1Interface
    {
        public MirageF1CEInterface() : base(
            "DCS Mirage F1EE",
            "Mirage-F1EE",
            "pack://application:,,,/Helios;component/Interfaces/DCS/MIRAGEF1/ExportFunctionsMirageF1EE.lua")
        {
            // see if we can restore from JSON
#if (!DEBUG)
                        if (LoadFunctionsFromJson())
                        {
                            return;
                        }
#endif
        }
    }
}
