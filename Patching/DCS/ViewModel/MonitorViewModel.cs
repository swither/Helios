// Copyright 2020 Ammo Goettsch
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
// 

using System;
using System.ComponentModel;
using System.Windows;
using GadrocsWorkshop.Helios.Util.Shadow;
using GadrocsWorkshop.Helios.Windows;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    /// <summary>
    /// view model for a monitor's DCS usage, such as whether it is included in the DCS resolution and how it is used
    /// when the menu is loaded versus when the simulation is running
    /// </summary>
    public class MonitorViewModel : HeliosViewModel<DCSMonitor>
    {
        #region Private

        private string _includedNarrative;
        private string _excludedNarrative;
        private Vector _globalOffset;
        private double _scale;

        #endregion

        internal MonitorViewModel(DCSMonitor monitor, Vector globalOffset, double scale)
            : base(monitor)
        {
            _scale = scale;
            _globalOffset = globalOffset;
            Update(monitor.Monitor);

            // changes to the Helios monitor geometry
            monitor.MonitorChanged += Monitor_MonitorChanged;

            // changes to our model
            monitor.PropertyChanged += Monitor_PropertyChanged;
        }

        // handle local view logic not involving other monitors
        // global logic is in MonitorSetupViewModel
        private void Monitor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Main":
                case "UserInterface":
                    if (Data.Permissions.HasFlag(DCSMonitor.PermissionsFlags.CanExclude) && !Data.HasContent)
                    {
                        Data.Included = false;
                        UpdateInclusionNarrative();
                    }
                    break;
            }
        }

        // not automatically called because we are strongly referenced by event
        internal void Dispose()
        {
            Data.MonitorChanged -= Monitor_MonitorChanged;
        }

        private void Monitor_MonitorChanged(object sender, RawMonitorEventArgs e)
        {
            // some geometry change may have happened to our monitor specifically
            Update(e.Raw);
        }

        internal void Update(Monitor monitor)
        {
            RawRect = new Rect(monitor.Left, monitor.Top, monitor.Width, monitor.Height);
            Update();
        }

        internal void Update(Vector globalOffset)
        {
            _globalOffset = globalOffset;
            Update();
        }

        internal void Update(double scale)
        {
            _scale = scale;
            Update();
        }

        private void Update()
        {
            Rect transformed = RawRect;
            transformed.Offset(_globalOffset);
            transformed.Scale(_scale, _scale);
            Rect = transformed;
            // ConfigManager.LogManager.LogDebug($"scaled monitor view {GetHashCode()} for monitor setup UI is {Rect}");
        }

        private void UpdateInclusionNarrative()
        {
            InclusionNarrative = Data.Included
                ? _includedNarrative
                : _excludedNarrative;
        }

        public Rect RawRect { get; set; }

        public static readonly DependencyProperty InclusionCanBeChangedProperty =
            DependencyProperty.Register("InclusionCanBeChanged", typeof(bool), typeof(MonitorViewModel),
                new PropertyMetadata(false));

        public bool InclusionCanBeChanged
        {
            get => (bool) GetValue(InclusionCanBeChangedProperty);
            set => SetValue(InclusionCanBeChangedProperty, value);
        }

        public static readonly DependencyProperty MainAssignmentCanBeChangedProperty =
            DependencyProperty.Register("MainAssignmentCanBeChanged", typeof(bool), typeof(MonitorViewModel),
                new PropertyMetadata(true));

        public bool MainAssignmentCanBeChanged
        {
            get => (bool) GetValue(MainAssignmentCanBeChangedProperty);
            set => SetValue(MainAssignmentCanBeChangedProperty, value);
        }

        public static readonly DependencyProperty UserInterfaceAssignmentCanBeChangedProperty =
            DependencyProperty.Register("UserInterfaceAssignmentCanBeChanged", typeof(bool), typeof(MonitorViewModel),
                new PropertyMetadata(true));

        public bool UserInterfaceAssignmentCanBeChanged
        {
            get => (bool) GetValue(UserInterfaceAssignmentCanBeChangedProperty);
            set => SetValue(UserInterfaceAssignmentCanBeChangedProperty, value);
        }

        public static readonly DependencyProperty InclusionNarrativeProperty =
            DependencyProperty.Register("InclusionNarrative", typeof(string), typeof(MonitorViewModel),
                new PropertyMetadata(null));

        public string InclusionNarrative
        {
            get => (string) GetValue(InclusionNarrativeProperty);
            set => SetValue(InclusionNarrativeProperty, value);
        }

        public static readonly DependencyProperty RectProperty =
            DependencyProperty.Register("Rect", typeof(Rect), typeof(MonitorViewModel), new PropertyMetadata(null));

        /// <summary>
        /// extent of monitor scaled for presentation in UI
        /// </summary>
        public Rect Rect
        {
            get => (Rect) GetValue(RectProperty);
            set => SetValue(RectProperty, value);
        }

        internal void UpdateFromPermissions()
        {
            if (!Data.Permissions.HasFlag(DCSMonitor.PermissionsFlags.CanInclude))
            {
                _includedNarrative = "UI Error: this monitor was supposed to be excluded due to monitor layout mode";
                _excludedNarrative = "This monitor can not be included due to the current monitor layout mode.";
                InclusionCanBeChanged = false;
                UserInterfaceAssignmentCanBeChanged = false;
                MainAssignmentCanBeChanged = false;
            }
            else 
            {
                UserInterfaceAssignmentCanBeChanged = true;
                MainAssignmentCanBeChanged = true;
                if (Data.HasContent)
                {
                    _includedNarrative = "This monitor must be included in the area drawn by DCS because there is content on it.";
                    _excludedNarrative = "UI Error: this monitor was supposed to be included due to content on it";
                    InclusionCanBeChanged = false;
                }
                else if (!Data.Permissions.HasFlag(DCSMonitor.PermissionsFlags.CanExclude))
                {
                    _includedNarrative = Data.Monitor.IsPrimaryDisplay ? 
                        "This monitor can not be excluded in the current monitor layout mode because it is configured as the primary display." : 
                        "This monitor can not be excluded due to the current monitor layout mode.";
                    _excludedNarrative = "UI Error: this monitor was supposed to be included due to monitor layout mode";
                    InclusionCanBeChanged = false;
                }
                else
                {
                    _includedNarrative = "This monitor can be removed from the area drawn by DCS because it is empty.";
                    _excludedNarrative = "This monitor can be added to the area drawn by DCS even though it currently does not contain any content.";
                    InclusionCanBeChanged = true;
                }
            }
            UpdateInclusionNarrative();
        }
    }
}