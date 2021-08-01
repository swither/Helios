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
    /// ramps a numeric value up and down, resting at the zero value for a while
    /// </summary>
    internal class NumericTester : TesterBase
    {
        private readonly double _range;
        private DateTime? _zeroTime;
        private const double RAMP_TIME = 2d; // number of seconds from 0 to max value (same amount of time taken to ramp back down and again to stay at 0)

        public NumericTester(DCSDataElement dataElement, int precision) : base(dataElement)
        {
            // NOTE: this doesn't work as we have plenty of values with high precision that use the whole range
            // _range = Math.Pow(10, 3 - precision);
            _range = 1d;
        }

        /// <summary>
        /// default tester for value with unknown precision
        /// </summary>
        /// <param name="dataElement"></param>
        public NumericTester(DCSDataElement dataElement) : base(dataElement)
        {
            _range = 1d;
        }

        public override string Update(DateTime now, TimeSpan elapsed)
        {
            double value;
            if (!_zeroTime.HasValue)
            {
                _zeroTime = now;
                value = 0;
            }
            else
            {
                TimeSpan rampSpan = now.Subtract(_zeroTime.Value);
                double x = rampSpan.TotalSeconds / RAMP_TIME;
                if (x <= 1.0)
                {
                    // ramping up
                    value = x * _range;
                }
                else if (x <= 2.0)
                {
                    // ramping down
                    value = (2 - x) * _range;
                }
                else if (x < 3.0)
                {
                    // rest at zero
                    value = 0d;
                }
                else
                {
                    // reset the cycle
                    value = 0d;
                    _zeroTime = now;
                }
            }

            return DCSInterfaceLoadTester.Format(Data.Format ?? "%.3f", value);
        }
    }
}