//  Copyright 2014 Craig Courtney
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

using System.Collections.Generic;

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// version 2 of ISettingsManager interface to add enumeration and removal
    /// </summary>
    public interface ISettingsManager2: ISettingsManager
    {
        /// <summary>
        /// enumerates all the keys in a specified group
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        IEnumerable<string> EnumerateSettingNames(string group);

        /// <summary>
        /// removes the setting with the specified key from the specified group
        /// </summary>
        /// <param name="group"></param>
        /// <param name="name"></param>
        void DeleteSetting(string group, string name);
    }
}