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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GadrocsWorkshop.Helios.Windows.Controls
{
    /// <summary>
    /// Editor for CalibrationPointCollectionDouble
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


        /// <summary>
        /// backing field for property AddCommand, contains
        /// handlers for "Add Point after Current" command
        /// </summary>
        private ICommand _addCommand;

        /// <summary>
        /// handlers for "Add Point after Current" command
        /// </summary>
        public ICommand AddCommand
        {
            get
            {
                _addCommand = _addCommand ?? new RelayCommand(
                    parameter =>
                    {
                        Calibration.AddPointAfter((CalibrationPointDouble)parameter);
                    },
                    parameter => !IsLast(parameter)
                );
                return _addCommand;
            }
        }

        /// <summary>
        /// backing field for property RemoveCommand, contains
        /// handlers for "Remove Point" command
        /// </summary>
        private ICommand _removeCommand;

        /// <summary>
        /// handlers for "Remove Point" command
        /// </summary>
        public ICommand RemoveCommand
        {
            get
            {
                _removeCommand = _removeCommand ?? new RelayCommand(
                    parameter =>
                    {
                        Calibration.Remove((CalibrationPointDouble) parameter);
                    },
                    parameter => (Calibration?.Count ?? 0) > 2 &&
                                 !IsLast(parameter) &&
                                 !IsFirst(parameter)
                );
                return _removeCommand;
            }
        }

        private bool IsFirst(object parameter)
        {
            return parameter != null && ((CalibrationPointDouble)parameter).Equals(Calibration.First());
        }

        private bool IsLast(object parameter)
        {
            return parameter != null && ((CalibrationPointDouble) parameter).Equals(Calibration.Last());
        }

        private void OnOutputChange(object sender, TextChangedEventArgs e)
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

        private void OnInputLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            Calibration?.Sort();
        }
    }
}