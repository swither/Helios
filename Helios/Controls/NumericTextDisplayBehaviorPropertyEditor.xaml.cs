//  Copyright 2014 Craig Courtney
//  Copyright 2021 Ammo Goettsch
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

using System.Collections.Generic;
using System.Linq;

namespace GadrocsWorkshop.Helios.Controls
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Windows.Controls;

    /// <summary>
    /// Editor for configuration specific to numeric text displays
    /// </summary>
    [HeliosPropertyEditor("Helios.Base.NumericTextDisplay", "Behavior")]
    public partial class NumericTextDisplayBehaviorEditor : HeliosPropertyEditor
    {
        public NumericTextDisplayBehaviorEditor()
        {
            InitializeComponent();
        }

        public class ComboRecord
        {
            public string DisplayName { get; internal set; }
            public string Value { get; internal set; }
        }

        /// <summary>
        /// filters units to those that make sense for this control
        /// </summary>
        public IEnumerable<BindingValueUnit> Units => BindingValueUnits.UnitNames
            .Select(BindingValueUnits.FetchUnitByName)
            .Where(unit => unit != null)
            .Where(unit =>
                // the numeric unit is tagged "other", but we want it
                // reject everything else that isn't a numeric unit of some type
                ReferenceEquals(unit, BindingValueUnits.Numeric) ||
                (unit.Type != BindingValueUnitType.Other && unit.Type != BindingValueUnitType.None));
    }
}
