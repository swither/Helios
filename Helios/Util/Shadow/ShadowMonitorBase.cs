// Copyright 2021 Ammo Goettsch
// 
// Patching is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Patching is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;

namespace GadrocsWorkshop.Helios.Util.Shadow
{
    public abstract class ShadowMonitorBase<TEvent> : ShadowVisual where TEvent: EventArgs
    {
        protected ShadowMonitorBase(IShadowVisualParent parent, Monitor monitor, HeliosVisual visual, bool recurse = true) : base(parent, monitor, visual, recurse)
        {
            // no code
        }

        /// <summary>
        /// deferred initialization so our factory can index this before we add children to it
        /// </summary>
        public abstract void Instrument();

        /// <summary>
        /// notification that a viewport was added to this monitor
        /// </summary>
        /// <returns></returns>
        public abstract bool AddViewport();

        /// <summary>
        /// notification that a viewport was removed from this monitor
        /// </summary>
        /// <returns></returns>
        public abstract bool RemoveViewport();

        /// <summary>
        /// create an EventArgs object that refers to this shadow monitor object
        /// </summary>
        /// <returns></returns>
        public abstract TEvent CreateEvent();
    }
}