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

namespace GadrocsWorkshop.Helios
{
    using System;
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Windows.Controls;

    /// <summary>
    /// factory of HeliosPropertyEditor class that has the attribute [HeliosCapabilityEditor]
    /// </summary>
    public class HeliosCapabilityEditorDescriptor
    {
        private readonly HeliosCapabilityEditorAttribute _editorAttribute;
        private readonly Type _editorType;

        public HeliosCapabilityEditorDescriptor(Type type, HeliosCapabilityEditorAttribute attribute)
        {
            _editorType = type;
            _editorAttribute = attribute;
        }

        public Type InterfaceType => _editorAttribute.InterfaceType;

        public string Category => _editorAttribute.Category;

        public HeliosPropertyEditor CreateInstance()
        {
            return (HeliosPropertyEditor)Activator.CreateInstance(_editorType);
        }
    }
}
