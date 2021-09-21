// Copyright 2014 Craig Courtney
// Copyright 2020 Helios Contributors
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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GadrocsWorkshop.Helios.Windows;
using GadrocsWorkshop.Helios.Windows.Controls;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    /// <summary>
    /// This DCS Interface editor can be used by descendants of DCSInterface that do not want to add any specific options.
    /// Using this class will avoid duplicating the XAML.
    ///
    /// NOTE: this class is its own DataContext
    /// </summary>
    public partial class DCSInterfaceEditor : DCSInterfaceEditorBase
    {
        static DCSInterfaceEditor()
        {
            RegisterCommands(typeof(DCSInterfaceEditor));
        }

        public DCSInterfaceEditor()
        {
            InitializeComponent();
        }
    }
}