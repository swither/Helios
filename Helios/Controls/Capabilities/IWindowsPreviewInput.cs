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

using System.Windows.Input;

namespace GadrocsWorkshop.Helios.Controls.Capabilities
{
    /// <summary>
    /// HeliosVisual controls that implement this interface are aware of Windows native
    /// APIs for mouse and touch events and will be called on the "Preview" pass of those
    /// events.
    ///
    /// This allows controls to be aware of when their child controls are clicked.
    /// </summary>
    public interface IWindowsPreviewInput
    {
        void PreviewMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs);
        void PreviewMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs);
        void PreviewTouchDown(object sender, TouchEventArgs touchEventArgs);
        void PreviewTouchUp(object sender, TouchEventArgs touchEventArgs);
    }
}