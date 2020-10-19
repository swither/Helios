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

namespace GadrocsWorkshop.Helios.Gauges.A10C.HARS
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using System;
    using System.Windows;
    using System.Windows.Media;

    [HeliosControl("Helios.A10C.HARSSync", "HARS Sync II", "A-10C Gauges", typeof(GaugeRenderer), HeliosControlFlags.NotShownInUI)]
    public class HARSSync : BaseGauge
    {
        private HeliosValue _syncOffset;
        private GaugeNeedle _needle;
        private CalibrationPointCollectionDouble _needleCalibration;

        public HARSSync()
            : base("HARS Sync", new Size(137d, 79d))
        {
            Components.Add(new GaugeImage("{A-10C}/Images/A-10C/A-10C_HARS_Scale_Panel.png", new Rect(0d, 0d, 137d, 79d)));

            _needleCalibration = new CalibrationPointCollectionDouble(-1d, -80d, 1d, 80d);
            _needle = new GaugeNeedle("{Helios}/Gauges/A-10/HARS/hars_needle.xaml", new Point(69d, 120d), new Size(20d, 75d), new Point(10d, 100d), 0d);
            _needle.Clip = new RectangleGeometry(new Rect(0d, 0d, 137d, 79d));
            Components.Add(_needle);

            _syncOffset = new HeliosValue(this, new BindingValue(0d), "", "sync offset", "Location of sync needle", "(-1 to 1)", BindingValueUnits.Numeric);
            _syncOffset.Execute += new HeliosActionHandler(Sync_Execute);
            Actions.Add(_syncOffset);
        }

        void Sync_Execute(object action, HeliosActionEventArgs e)
        {
            _needle.Rotation = _needleCalibration.Interpolate(e.Value.DoubleValue);
        }
    }
}
