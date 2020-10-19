//  Copyright 2014 Craig Courtney
//  Copyright 2020 Helios Contributors
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

namespace GadrocsWorkshop.Helios.Gauges.AV8B
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using System;
    using System.Windows;
    using System.Windows.Media;

    [HeliosControl("Helios.AV8B.stabilizerDisplay", "Stabilizer Direction Display", "AV-8B Gauges", typeof(GaugeRenderer), HeliosControlFlags.NotShownInUI)]
    public class StabilizerDisplay : BaseGauge
    {
        private readonly GaugeDrumCounter _onesDrum;

        public StabilizerDisplay()
            : this("Stabilizer Direction",new Point(0d, 0d), new Size(40d, 50d))
        {
        }
        public StabilizerDisplay(String componentName, Point position, Size size)
            : base(componentName, size)
        {
            //Components.Add(new GaugeImage("{Helios}/Gauges/AV-8B/stabilizer display/digit_faceplate.xaml", new Rect(0d, 0d, 80d, 100d)));

            _onesDrum = new GaugeDrumCounter("{Helios}/Gauges/AV-8B/stabiliser display/stabilizer_drum_tape.xaml", position, "#", new Size(8d, 10d), size);
            _onesDrum.Clip = new RectangleGeometry(new Rect(0d, 0d, size.Width, size.Height));
            Components.Add(_onesDrum);

            HeliosValue oneDigitDisplay = new HeliosValue(this, new BindingValue(0d), "", "value", "EDP", "Stabilizer direction display", BindingValueUnits.Numeric);
            oneDigitDisplay.Execute += DigitDisplay_Execute;
            Actions.Add(oneDigitDisplay);
        }

        public void DigitDisplay_Execute(object action, HeliosActionEventArgs e)
        {
            _onesDrum.Value = e.Value.DoubleValue + 2;  // The adddition of the +2 is to cater for the fact that the value is -1 to 1
        }
    }
}
