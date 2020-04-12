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

namespace GadrocsWorkshop.Helios.ControlCenter
{
    using System;
    using System.Windows.Input;
    
    static class ControlCenterCommands
    {
        public static RoutedUICommand NextProfile { get; } = new RoutedUICommand("Next Profile", "NextProfile", typeof(ControlCenterCommands));
        public static RoutedUICommand PreviousProfile { get; } = new RoutedUICommand("Previous Profile", "PreviousProfile", typeof(ControlCenterCommands));
        public static RoutedUICommand StartProfile { get; } = new RoutedUICommand("Start Profile", "StartProfile", typeof(ControlCenterCommands));
        public static RoutedUICommand StopProfile { get; } = new RoutedUICommand("Stop Profile", "StopProfile", typeof(ControlCenterCommands));
        public static RoutedUICommand ResetProfile { get; } = new RoutedUICommand("Reset Profile", "ResetProfile", typeof(ControlCenterCommands));
        public static RoutedUICommand DeleteProfile { get; } = new RoutedUICommand("Delete Profile", "DeleteProfile", typeof(ControlCenterCommands));
        public static RoutedUICommand OpenControlCenter { get; } = new RoutedUICommand("Open Control Center", "OpenControlCenter", typeof(ControlCenterCommands));
        public static RoutedUICommand TogglePreferences { get; } = new RoutedUICommand("Toggle Display of Control Center Preferences", "TogglePreferences", typeof(ControlCenterCommands));
        public static RoutedUICommand RunProfile { get; } = new RoutedUICommand("Open and Run a Profile", "RunProfile", typeof(ControlCenterCommands));

        public static RoutedUICommand ResetCaution { get; } = new RoutedUICommand("Reset Caution Light", "ResetCaution", typeof(ControlCenterCommands));
    }
}
