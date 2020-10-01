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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.A10C
{
    using ComponentModel;
    using Common;

    /// <summary>
    /// interface for DCS A-10C II including any changes made that are not in DCS A-10C.   Please note the poor naming
    /// of the Helios interface (A10C instead of A-10C) is intentional to match the existing interface.
    /// </summary>
    [HeliosInterface("Helios.A10C2", "DCS A10C II", typeof(DCSInterfaceEditor), typeof(UniqueHeliosInterfaceFactory))]
    class A10C2Interface: A10CInterface
    {
        public A10C2Interface() : base(
            "DCS A10C II", 
            "A-10C_2",
            "pack://application:,,,/Helios;component/Interfaces/DCS/A10C/ExportFunctionsA10C2.lua")
        {
            // TODO: additional functions only in A-10C II go here
        }
    }
}
