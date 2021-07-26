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
    public interface ILinearControl
    {
        #region Properties

        /// <summary>
        /// the position of the control as a multiple of its declared range, so that
        /// 1.0 is the configured maximum for the control and 0.0 is the configured minimum
        /// setting the value outside the range must be handled gracefully by the control by
        /// wrapping or clamping, depending on the type of control it is
        /// </summary>
        double ControlPosition { get; set; }

        #endregion
    }
}