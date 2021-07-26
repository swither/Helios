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

namespace GadrocsWorkshop.Helios.Controls.Capabilities
{
    public interface IPulsedControl
    {
        #region Hooks

        /// <summary>
        /// increment (positive pulses) or decrement (negative pulses) the control
        /// </summary>
        /// <param name="pulses">the number of pulses, negative if decrementing</param>
        void Pulse(int pulses);

        #endregion
    }
}