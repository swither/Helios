﻿//  Copyright 2014 Craig Courtney
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

namespace GadrocsWorkshop.Helios.Gauges.F5E.Instruments.VVI
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using System;
    using System.Windows;

    [HeliosControl("Helios.F5E.Instruments.VVI", "Vertical Velocity Indicator", "F-5E Tiger II", typeof(GaugeRenderer), HeliosControlFlags.NotShownInUI)]
    public class VVIGauge : BaseGauge
    {
        private HeliosValue _verticalVelocity;
        private GaugeNeedle _needle;
        private CalibrationPointCollectionDouble _calibrationPoints;

        public VVIGauge(): base("Vertical Velocity Indicator", new Size(300d,300d))
        {
            double scalingFactor = this.Height / NativeSize.Height;
            scalingFactor = 1d;
            Components.Add(new GaugeImage("{F-5E}/Gauges/Instruments/VVI-Faceplate.xaml", new Rect(0d, 0d, this.Height, this.Width)));
            _needle = new GaugeNeedle("{F-5E}/Gauges/Instruments/VVI-Needle.xaml", new Point(this.Height/2, this.Width/2), new Size(31.159d * scalingFactor, 190 * scalingFactor), new Point(16.080 * scalingFactor, 128.658 * scalingFactor), -90d);
            Components.Add(_needle);
            _verticalVelocity = new HeliosValue(this, new BindingValue(0d), $"Instruments_Vertical Velocity Indicator", "Vertical Velocity Indicator", "Veritcal velocity of the aircraft", "(-6,000 to 6,000)", BindingValueUnits.FeetPerMinute);
            _verticalVelocity.Execute += new HeliosActionHandler(VerticalVelocity_Execute);
            Actions.Add(_verticalVelocity);

            _calibrationPoints = new CalibrationPointCollectionDouble(-6000d, -169d, 6000d, 169d)
            {
                new CalibrationPointDouble(-4000d, -140d),
                new CalibrationPointDouble(-2000d, -102d),
                new CalibrationPointDouble(-1000d, -67d),
                new CalibrationPointDouble(-500d, -37d),
                new CalibrationPointDouble(0d, 0d),
                new CalibrationPointDouble(500d, 37d),
                new CalibrationPointDouble(1000d, 67d),
                new CalibrationPointDouble(2000d, 102d),
                new CalibrationPointDouble(4000d, 140d)
            };
        }
        void VerticalVelocity_Execute(object action, HeliosActionEventArgs e)
        {
            _needle.Rotation = _calibrationPoints.Interpolate(e.Value.DoubleValue);
        }

    }
}
