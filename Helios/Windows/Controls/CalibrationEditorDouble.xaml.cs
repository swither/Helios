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

using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace GadrocsWorkshop.Helios.Windows.Controls
{
    /// <summary>
    /// Interaction logic for CalibrationEditorDouble.xaml
    /// </summary>
    public partial class CalibrationEditorDouble : UserControl
    {
        public CalibrationEditorDouble()
        {
            InitializeComponent();
        }

        public CalibrationPointCollectionDouble Calibration
        {
            get => (CalibrationPointCollectionDouble) GetValue(CalibrationProperty);
            set => SetValue(CalibrationProperty, value);
        }

        public static readonly DependencyProperty CalibrationProperty =
            DependencyProperty.Register("Calibration", typeof(CalibrationPointCollectionDouble),
                typeof(CalibrationEditorDouble), new UIPropertyMetadata(null));

        public double MaxOutput
        {
            get => (double) GetValue(MaxOutputProperty);
            set => SetValue(MaxOutputProperty, value);
        }

        public static readonly DependencyProperty MaxOutputProperty =
            DependencyProperty.Register("MaxOutput", typeof(double), typeof(CalibrationEditorDouble),
                new UIPropertyMetadata(double.MaxValue));

        public double MinOutput
        {
            get => (double) GetValue(MinOutputProperty);
            set => SetValue(MinOutputProperty, value);
        }

        public static readonly DependencyProperty MinOutputProperty =
            DependencyProperty.Register("MinOutput", typeof(double), typeof(CalibrationEditorDouble),
                new UIPropertyMetadata(double.MinValue));

        private void AddCalibrationPoint(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Calibration.AddPointAfter((CalibrationPointDouble) button.Tag);
        }

        private void RemoveCalibrationPoint(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Calibration.Remove((CalibrationPointDouble) button.Tag);
        }

        private void OutputChange(object sender, TextChangedEventArgs e)
        {
            if (!(sender is HeliosTextBox box))
            {
                return;
            }

            if (!double.TryParse(box.Text, out double value))
            {
                value = 0d;
                // fall through to clamp value
            }

            if (value < MinOutput)
            {
                box.Text = MinOutput.ToString(CultureInfo.CurrentCulture);
            }
            else if (value > MaxOutput)
            {
                box.Text = MaxOutput.ToString(CultureInfo.CurrentCulture);
            }
        }
    }
}