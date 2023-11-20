// Copyright 2023 Helios contributors
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
    public interface IRefreshableImage
    {
        /// <summary>
        /// Determines if an image is used by a control, and calls refresh (to allow the renderer 
        /// an opportunity to reload the image from disk)
        /// </summary>
        /// <param name="imageName">The name of the image within the Helios ImagePath</param>
        /// <returns>true if imageName was found</returns>
        bool ConditionalImageRefresh(string imageName);

    }
}