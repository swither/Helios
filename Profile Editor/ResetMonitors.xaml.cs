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

namespace GadrocsWorkshop.Helios.ProfileEditor
{
    using GadrocsWorkshop.Helios.ProfileEditor.ViewModel;
    using System.Collections.Generic;
    using System.Windows;

    /// <summary>
    /// Reset Monitors dialog shows old and new layout of monitors and controls on them
    /// </summary>
    public partial class ResetMonitors : Window
    {
        public ResetMonitors(HeliosProfile profile)
        {
            HeliosProfile newProfile = new HeliosProfile();

            MonitorResets = new List<MonitorResetItem>(profile.Monitors.Count);
            for (int i = 0; i < profile.Monitors.Count; i++)
            {
                MonitorResets.Add(new MonitorResetItem(profile.Monitors[i], i, i < newProfile.Monitors.Count ? i : 0));
            }

            NewMonitors = new List<string>(newProfile.Monitors.Count);
            foreach (Monitor monitor in newProfile.Monitors)
            {
                NewMonitors.Add(monitor.Name);
            }

            InitializeComponent();

            OldLayout.Profile = profile;
            NewLayout.Profile = newProfile;
        }

        public List<MonitorResetItem> MonitorResets { get; }

        public List<string> NewMonitors { get; }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Ok(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
