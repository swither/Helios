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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.MIRAGEF1
{
    //using Functions;
    using Common;

    /// <summary>
    /// common base for interfaces for DCS MirageF1CE and MirageF1EE, containing only functions that are the
    /// same in both aircraft
    /// </summary>
    public class MirageF1Interface : DCSInterface
    {
        protected MirageF1Interface(string heliosName, string dcsVehicleName, string exportFunctionsUri)
            : base(heliosName, dcsVehicleName, exportFunctionsUri)
        {
        }
    }
}
