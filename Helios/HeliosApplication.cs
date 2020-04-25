//  Copyright 2014 Craig Courtney
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// application behavior that differs between different Helios executables
    /// and that controls and other Helios components may need to know about
    /// </summary>
    public class HeliosApplication
    {
        /// <summary>
        /// if true, controls should show their design-time views, as opposed to the
        /// way controls are rendered at run time when being driven off of Simulator data
        /// 
        /// this value is stable for the entire execution of any Helios Application and it can
        /// be accessed at any time after HeliosInit.Initialize
        /// </summary>
        public bool ShowDesignTimeControls { get; set; } = true;

        /// <summary>
        /// if set, allows loading of Helios modules from ./Plugins relative to the installation folder
        /// </summary>
        public bool AllowPlugins { get; set; } = true;

        /// <summary>
        /// if set, interfaces should attempt to connect to remote servers for runtime operation
        /// </summary>
        public bool ConnectToServers { get; set; } = false;

        /// <summary>
        /// if set, this application is allowed to write to the global Helios SettingsManager, rather than just
        /// read from it
        /// </summary>
        public bool SettingsAreWritable { get; set; } = true;
    }
}