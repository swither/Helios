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

using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Windows.Controls;
using System;
using System.Windows;

namespace GadrocsWorkshop.Helios.Controls.Special
{
    /// <summary>
    /// Interaction logic for ScreenReplicatorAdjustableAppearancePropertyEditor.xaml
    /// </summary>
    [HeliosPropertyEditor("Helios.Base.ScreenReplicatorAdjustable", "Appearance")]
    public partial class ScreenReplicatorAdjustableAppearancePropertyEditor : HeliosPropertyEditor
    {
        public ScreenReplicatorAdjustableAppearancePropertyEditor()
        {
            InitializeComponent();
        }
        private void BrightnessSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.textBlockBrightnessValue.Text = $"{e.NewValue:N2}";
        }
        private void RedBrightnessSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.textBlockRedBrightnessValue.Text = $"{e.NewValue:N2}";
        }
        private void GreenBrightnessSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.textBlockGreenBrightnessValue.Text = $"{e.NewValue:N2}";
        }
        private void BlueBrightnessSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.textBlockBlueBrightnessValue.Text = $"{e.NewValue:N2}";
        }
        private void ContrastSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(textBlockContrastValue != null)
            {
                this.textBlockContrastValue.Text = $"{e.NewValue:N2}";
            }
        }
        private void GammaSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(this.textBlockGammaValue != null)
            {
                this.textBlockGammaValue.Text = $"{e.NewValue:N2}";
            }
        }
    }
}
