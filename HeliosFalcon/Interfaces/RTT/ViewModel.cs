// Copyright 2021 Ammo Goettsch
// 
// HeliosFalcon is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// HeliosFalcon is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using GadrocsWorkshop.Helios.Windows;
using System.Windows;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.Interfaces.RTT
{
    /// <summary>
    /// view model that only exists to enable/disable controls based on checkbox that is used to configure model boolean
    /// </summary>
    public class ViewModel : HeliosViewModel<ConfigGenerator>
    {
        public ViewModel(ConfigGenerator data) : base(data)
        {
            // initial configuration state
            RttEnabled = Data.Enabled;
            NetworkingEnabled = Data.Networked;
        }

        public bool RttEnabled
        {
            get => (bool) GetValue(RttEnabledProperty);
            set => SetValue(RttEnabledProperty, value);
        }

        public static readonly DependencyProperty RttEnabledProperty =
            DependencyProperty.Register("RttEnabled", typeof(bool), typeof(ViewModel), new PropertyMetadata(false,
                (d, e) =>
                {
                    // forward to model for persistence
                    if (d is ViewModel thisOne)
                    {
                        thisOne.Data.Enabled = (bool) e.NewValue;
                    }
                }));

        public bool NetworkingEnabled
        {
            get => (bool) GetValue(NetworkingEnabledProperty);
            set => SetValue(NetworkingEnabledProperty, value);
        }

        public static readonly DependencyProperty NetworkingEnabledProperty =
            DependencyProperty.Register("NetworkingEnabled", typeof(bool), typeof(ViewModel), new PropertyMetadata(
                false, (d, e) =>
                {
                    // forward to model for persistence
                    if (d is ViewModel thisOne)
                    {
                        thisOne.Data.Networked = (bool) e.NewValue;
                    }
                }));
    }
}