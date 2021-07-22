// Copyright 2021 Ammo Goettsch
// 
// HeliosFalcon is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// HeliosFalcon is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using GadrocsWorkshop.Helios.Util.Shadow;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.Interfaces.RTT
{
    /// <summary>
    /// specific monitor class used in this implementation; add any additional information that is
    /// required on a per-monitor basis, if any
    /// </summary>
    public class ShadowMonitor : ShadowMonitorBase<ShadowMonitorEventArgs>
    {
        public ShadowMonitor(IShadowVisualParent parent, Monitor monitor, HeliosVisual visual, bool recurse = true) :
            base(parent, monitor, visual, recurse)
        {
            // TODO: load any saved per-monitor configuration
        }

        #region Overrides

        public override bool AddViewport() => true;

        public override ShadowMonitorEventArgs CreateEvent() => new ShadowMonitorEventArgs(this);

        public override void Instrument()
        {
            // build the tree from this monitor
            Instrument(Monitor, Monitor);
        }

        public override bool RemoveViewport() => true;

        #endregion
    }


    /// <summary>
    /// don't modify this class; it is just a fully typed event for the specific monitor class used here
    /// </summary>
    public class ShadowMonitorEventArgs : EventArgs
    {
        public ShadowMonitorEventArgs(ShadowMonitor sampleMonitor)
        {
            ShadowMonitor = sampleMonitor;
        }

        #region Properties

        public ShadowMonitor ShadowMonitor { get; }

        #endregion
    }
}