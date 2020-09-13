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

using GadrocsWorkshop.Helios.Tools.Capabilities;

namespace GadrocsWorkshop.Helios.ProfileEditorTools
{
    /// <summary>
    /// base class for profile tools that retain a profile reference
    /// </summary>
    public class ProfileTool : IProfileTool
    {
        public virtual bool CanStart => Profile != null;

        protected HeliosProfile Profile { get; private set; }

        public virtual void Open(HeliosProfile newProfile)
        {
            Profile = newProfile;
        }

        public virtual void Close(HeliosProfile oldProfile)
        {
            Profile = null;
        }
    }
}