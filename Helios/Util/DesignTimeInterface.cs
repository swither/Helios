// Copyright 2021 Ammo Goettsch
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

namespace GadrocsWorkshop.Helios.Util
{
    /// <summary>
    /// a design instance for an editor of a helios interface, because "Interface" is in the paths
    /// in all legacy code instead of making "Control" the data context
    ///
    /// using a concretized descendant of this class as the design instance in an interface editor will allow path
    /// checking by XAML Intellisense
    ///
    /// example XAML:
    ///
    ///     d:DataContext="{d:DesignInstance Type={x:Type controls:DesignTimeMyInterfaceName}}"
    ///
    /// with C#:
    ///
    ///     DesignTimeMyInterfaceName: DesignTimeControl&lt;MyInterfaceName&gt;
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DesignTimeInterface<T> where T: HeliosInterface
    {
        public DesignTimeInterface()
        {
            Interface = Activator.CreateInstance<T>();
        }

        public T Interface { get; }
    }
}