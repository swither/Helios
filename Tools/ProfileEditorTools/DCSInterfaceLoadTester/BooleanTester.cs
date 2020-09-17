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

using GadrocsWorkshop.Helios.Interfaces.DCS.Common;
using System;

namespace GadrocsWorkshop.Helios.ProfileEditorTools.DCSInterfaceLoadTester
{
    /// <summary>
    /// periodically toggles back and forth between 0 and 1
    /// </summary>
    internal class BooleanTester : TesterBase
    {
        private bool _value;

        public BooleanTester(DCSDataElement data) : base(data)
        {
            // no code
        }

        public override string Update(DateTime now, TimeSpan elapsed)
        {
            bool value = (now.Second % 2) == 0;
            if (_value == value)
            {
                return null;
            }

            _value = value;
            return DCSInterfaceLoadTester.Format(Data.Format, value ? 1d : 0d);
        }
    }
}