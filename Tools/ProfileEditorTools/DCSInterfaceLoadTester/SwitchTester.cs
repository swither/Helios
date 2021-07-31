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
using System.Collections.Generic;
using System.Linq;

namespace GadrocsWorkshop.Helios.ProfileEditorTools.DCSInterfaceLoadTester
{
    /// <summary>
    /// steps through all the valid argument values for a Switch control
    /// </summary>
    internal class SwitchTester : TesterBase
    {
        private DateTime? _timeChanged;
        private const double CHANGE_INTERVAL = 1d;
        private readonly List<string> _values;
        private int _currentIndex;

        public SwitchTester(DCSDataElement dataElement, Switch switchFunction) : base(dataElement)
        {
            _values = switchFunction.Positions.Select(p => p.ArgValue).ToList();
        }

        public override string Update(DateTime now, TimeSpan elapsed)
        {
            if (_timeChanged.HasValue && now.Subtract(_timeChanged.Value).TotalSeconds < CHANGE_INTERVAL)
            {
                return null;
            }

            _timeChanged = now;
            if (!_values.Any())
            {
                return null;
            }

            string result = _values[_currentIndex];
            _currentIndex = (_currentIndex + 1) % _values.Count;
            return result;
        }
    }
}