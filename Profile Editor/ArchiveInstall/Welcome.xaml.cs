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

using AvalonDock.Layout;
using System;
using System.Windows.Controls;

namespace GadrocsWorkshop.Helios.ProfileEditor.ArchiveInstall
{
    /// <summary>
    /// Welcome screen for archive installation mode
    /// </summary>
    public partial class Welcome : Grid
    {
        public const string CONTENT_ID = "InstallationModeWelcome";

        public Welcome()
        {
            InitializeComponent();
            Name = CONTENT_ID;
        }

        internal static void Create(LayoutAnchorable container, out object content)
        {
            if (container == null)
            {
                content = null;
                return;
            }
            container.Title = "Installation Mode";
            container.CanClose = false;
            container.CanHide = false;
            content = new Welcome();
            container.Content = content;
        }
    }
}
