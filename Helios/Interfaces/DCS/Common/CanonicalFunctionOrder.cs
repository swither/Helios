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
using System.Collections.Generic;
using System.Linq;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    /// <summary>
    /// custom comparer that results in the same order of functions that we use in generated interfaces
    /// for ease of merging
    /// 
    /// ascending by device
    /// then ascending by primary argument/id
    /// then ascending by key or name depending on the source
    /// 
    /// </summary>
    internal class CanonicalFunctionOrder : IComparer<UDPInterface.NetworkFunction>
    {
        #region Implementation of IComparer<in NetworkFunction>

        public int Compare(UDPInterface.NetworkFunction leftBase, UDPInterface.NetworkFunction rightBase)
        {
            // NOTE: we don't want to cast this multiple times, so not pattern matching
            DCSFunction right = rightBase as DCSFunction;

            if (leftBase is DCSFunction left) {
                if (right == null)
                {
                    // DCS functions go first, other ones are sorted only by their first export if any
                    return -1;
                }

                // compare in the same order we use for generated interface
                int deviceOrder = string.Compare(left.DeviceName, right.DeviceName, StringComparison.InvariantCulture);
                if (deviceOrder != 0)
                {
                    // sorted by device
                    return deviceOrder;
                }

                // NOTE this is in string order, even though ID is frequently set to the string representation of an integer for Lua performance,
                // it is actually a string and we have non-numeric IDs
                int idOrder = string.Compare(left.DataElements.FirstOrDefault()?.ID ?? "",
                    right.DataElements.FirstOrDefault()?.ID ?? "", StringComparison.InvariantCulture);
                if (idOrder != 0)
                {
                    // sorted by id
                    return idOrder;
                }

                // sort by name, if any
                return string.Compare(left.Name, right.Name, StringComparison.InvariantCulture);
            }

            if (right != null)
            {
                // non-DCS functions go after any DCS functions
                return 1;
            }
            
            // all non-DCS functions are in order of their exports
            if (leftBase != null)
            {
                if (rightBase != null)
                {
                    return string.Compare(leftBase.LocalKey, rightBase.LocalKey, StringComparison.InvariantCulture);
                }

                return 1;
            }

            if (rightBase != null)
            {
                return -1;
            }

            // both are null
            return 0;
        }

        #endregion
    }
}