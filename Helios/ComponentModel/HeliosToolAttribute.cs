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

namespace GadrocsWorkshop.Helios.ComponentModel
{
    /// <summary>
    /// classes that have this attribute attached can be instantiated as tools that operate
    /// on a HeliosProfile object in a privileged way
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class HeliosToolAttribute: Attribute
    {
        // no code, since no values need to be provided, any tool functionality is detected
        // by probing for interfaces from Helios.Tools.Capabilities
    }
}
