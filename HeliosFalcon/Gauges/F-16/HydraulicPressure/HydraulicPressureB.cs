//  Copyright 2014 Craig Courtney
//  Copyright 2022 Helios Contributors
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

namespace GadrocsWorkshop.Helios.Gauges.Falcon.HydraulicPressure
{
	using GadrocsWorkshop.Helios.ComponentModel;
	using System.Windows;

	[HeliosControl("Helios.Falcon.HydraulicPressureB", "Falcon BMS Hydraulic Pressure B", "Falcon BMS F-16", typeof(GaugeRenderer))]
	public class HydraulicPressureB : BaseHydraulicPressure
	{
		public HydraulicPressureB()
			: base("Hydraulic Pressure B", new Size(300, 300))
		{
			this.InputBinding = "Pressure B";
		}
	}
}
