//  Copyright 2014 Craig Courtney
//  Copyright 2020 Ammo Goettsch
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

using System;

namespace GadrocsWorkshop.Helios.ComponentModel
{
    /// <summary>
    /// A property editor that will be used for any control that implements the specified interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class HeliosCapabilityEditorAttribute : HeliosEditorAttribute
    {
        /// <param name="interfaceInterfaceType">typeof(InterfaceTypeName)</param>
        /// <param name="category">Display name used for this section in the ui.</param>
        public HeliosCapabilityEditorAttribute(Type interfaceInterfaceType, string category)
            : base(category)
        {
            InterfaceType = interfaceInterfaceType;
        }

        public Type InterfaceType { get; }
    }
}