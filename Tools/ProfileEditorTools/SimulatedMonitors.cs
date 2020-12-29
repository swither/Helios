// Copyright 2020 Ammo Goettsch
// 
// ProfileEditorTools is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ProfileEditorTools is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System.Collections.Generic;
using System.Linq;
using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Interfaces.Capabilities;
using GadrocsWorkshop.Helios.Tools;
using GadrocsWorkshop.Helios.Tools.Capabilities;

namespace GadrocsWorkshop.Helios.ProfileEditorTools
{
    [HeliosTool]
    internal class SimulatedMonitors : ProfileTool, IMenuSectionFactory
    {
        private void Reset()
        {
            ConfigManager.DisplayManager.StopSimulating();
            IsSimulating = false;
            if (Profile == null)
            {
                return;
            }

            UpdateProfile();
        }

        private void UpdateProfile()
        {
            Profile.InvalidateLayoutCheck();
            foreach (IStatusReportNotify status in Profile.Interfaces.OfType<IStatusReportNotify>())
            {
                status.InvalidateStatusReport();
            }
        }

        private void Simulate()
        {
            ConfigManager.DisplayManager.Simulate(Profile.Monitors);
            UpdateProfile();
            IsSimulating = true;
        }

        #region IMenuSectionFactory

        public MenuSectionModel CreateMenuSection()
        {
            return new MenuSectionModel("Cascade Remover", new List<MenuItemModel>
            {
                new MenuItemModel("Simulate monitor layout stored in current profile", new Windows.RelayCommand(
                    parameter => { Simulate(); },
                    parameter => CanStart)),
                new MenuItemModel("Stop simulating monitor layout", new Windows.RelayCommand(
                    parameter => { Reset(); },
                    parameter => IsSimulating))
            });
        }

        #endregion

        #region Properties

        public bool IsSimulating { get; set; }

        #endregion
    }
}