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
using GadrocsWorkshop.Helios.Interfaces.DCS.Common;

namespace GadrocsWorkshop.Helios.ProfileEditorTools.DCSInterfaceLoadTester
{
    /// <summary>
    /// base class for a tester that creates test data appropriate for a specific DCS data element (id, value)
    /// </summary>
    internal abstract class TesterBase : IDisposable
    {
        public DCSDataElement Data { get; }

        protected TesterBase(DCSDataElement data)
        {
            Data = data;
        }

        public void Dispose()
        {
            // no code in base
        }

        /// <summary>
        /// calculates the changed value that we should dispatch to the data element's handler
        /// </summary>
        /// <param name="now"></param>
        /// <param name="elapsed"></param>
        /// <returns>a synthesized value in the format it would have from the network, or null if no value should be dispatched now</returns>
        public abstract string Update(DateTime now, TimeSpan elapsed);
    }
}