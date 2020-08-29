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

using System.Windows;

namespace GadrocsWorkshop.Helios.ProfileEditor.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MonitorResetItem : NotificationObject
    {
        private readonly int _oldId;
        private int _newMonitor;
        private readonly double _oldWidth;
        private readonly double _oldHeight;

        public MonitorResetItem(Monitor oldMonitor, int oldId, int newId)
        {
            Controls = new List<HeliosVisual>();
            OldMonitor = oldMonitor;
            _oldId = oldId;
            _newMonitor = newId;
            _oldWidth = oldMonitor.Width;
            _oldHeight = oldMonitor.Height;
        }

        public ResetMonitorsScalingMode ScalingMode { get; set; } = ResetMonitorsScalingMode.ScaleMonitor;

        public List<HeliosVisual> Controls { get; }

        public Monitor OldMonitor { get; }

        public int NewMonitor
        {
            get
            {
                return _newMonitor;
            }
            set
            {
                if (!_newMonitor.Equals(value))
                {
                    int oldValue = _newMonitor;
                    _newMonitor = value;
                    OnPropertyChanged("NewMonitor", oldValue, value, false);
                }
            }
        }

        public IEnumerable<string> Reset()
        {
            Monitor display = ConfigManager.DisplayManager.Displays[_oldId];
            ChooseTransform(display, out double scale,  out Point translation);
            
            // atomically change the size of the monitor to match the local display
            OldMonitor.CopyGeometry(display);

            foreach (HeliosVisual visual in OldMonitor.Children)
            {
                yield return UpdateControl(OldMonitor, visual, scale, translation);
            }
        }

        private string UpdateControl(Monitor monitor, HeliosVisual visual, double scale, Point translation, string commentPrefix = "")
        {
            switch (ScalingMode)
            {
                case ResetMonitorsScalingMode.None:
                    CheckBounds(visual, monitor);
                    return $"{commentPrefix}checked bounds of {visual.TypeIdentifier} {visual.Name}";
                case ResetMonitorsScalingMode.ScaleMonitor:
                    TransformControl(visual, scale, translation);
                    return $"{commentPrefix}scaled {visual.TypeIdentifier} {visual.Name}";
                case ResetMonitorsScalingMode.ScaleToFit:
                    TransformControl(visual, scale, translation);
                    return $"{commentPrefix}scaled {visual.TypeIdentifier} {visual.Name} to fit";
                case ResetMonitorsScalingMode.ScaleToTopRightQuarter:
                    TransformControl(visual, scale, translation);
                    return $"{commentPrefix}scaled {visual.TypeIdentifier} {visual.Name} for demo";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IEnumerable<string> RemoveControls()
        {
            HeliosVisual[] children = OldMonitor.Children.ToArray();
            foreach (HeliosVisual visual in children)
            {
                Controls.Add(visual);
                OldMonitor.Children.Remove(visual);
                yield return $"lifted {visual.TypeIdentifier} {visual.Name}";
            }
        }

        public IEnumerable<string> PlaceControls(Monitor newMonitor)
        {
            if (!Controls.Any())
            {
                yield break;
            }
            ChooseTransform(newMonitor, out double scale, out Point translation);
            foreach (HeliosVisual visual in Controls)
            {
                // Make sure name is unique
                int i = 1;
                string name = visual.Name;
                while (newMonitor.Children.ContainsKey(name))
                {
                    name = visual.Name + " " + i++;
                }
                visual.Name = name;

                newMonitor.Children.Add(visual);

                yield return UpdateControl(newMonitor, visual, scale, translation, "placed and ");
            }
        }

        /// <summary>
        /// choose best scaling factor and translation vector
        /// </summary>
        /// <returns></returns>
        private void ChooseTransform(Monitor newMonitor, out double scale, out Point translation)
        {
            switch (ScalingMode)
            {
                case ResetMonitorsScalingMode.None:
                    scale = 1d;
                    translation = new Point(0d, 0d);
                    break;

                case ResetMonitorsScalingMode.ScaleMonitor:
                    // scale based only on monitor sizes
                    scale = Math.Min(newMonitor.Width / _oldWidth, newMonitor.Height / _oldHeight);
                    translation = new Point(0d, 0d);
                    break;

                case ResetMonitorsScalingMode.ScaleToFit:
                    // calculate the visible extent of the controls being placed, including top left corner
                    Rect extent = Rect.Empty;
                    foreach (HeliosVisual visual in Controls.Concat(OldMonitor.Children))
                    {
                        extent.Union(visual.DisplayRectangle);
                    }
                    extent.Intersect(new Rect(0, 0, _oldWidth, _oldHeight));

                    // NOTE: populated extent should generally be much larger than 2 pixels, we are just using this here to protect against rounding issues
                    if ((extent.Width >= 2d) && (extent.Height >= 2d))
                    {
                        // choose a scaling factor that will maximize the content while preserving aspect
                        // (this allows going back from portrait to landscape, for example)
                        scale = Math.Min(newMonitor.Height / extent.Height, newMonitor.Width / extent.Width);
                        translation = new Point(-extent.Left * scale, -extent.Top * scale);
                    }
                    else
                    {
                        // scale based only on monitor sizes
                        scale = Math.Min(newMonitor.Width / _oldWidth, newMonitor.Height / _oldHeight);
                        translation = new Point(0d, 0d);
                    }
                    break;

                case ResetMonitorsScalingMode.ScaleToTopRightQuarter:
                    scale = Math.Min(newMonitor.Width / _oldWidth, newMonitor.Height / _oldHeight) / 2d;
                    translation = new Point(newMonitor.Width / 2d, 0d);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void CopySettings(Monitor newMonitor)
        {
            if (Controls.Count == 0)
            {
                // nothing transferred
                // NOTE: this also covers the case where the source and target monitor are the same
                return;
            }
            if (!OldMonitor.FillBackground)
            {
                // the transferred controls require a transparent monitor,
                // so we have to set this, even if it gets combined with controls
                // from opaque monitors
                newMonitor.FillBackground = false;
            }
            if (OldMonitor.AlwaysOnTop)
            {
                // even if we combine multiple monitors,
                // in order to have any of the source monitors on top, we 
                // need to set the combined monitor on top
                newMonitor.AlwaysOnTop = true;
            }
        }

        private void CheckBounds(HeliosVisual visual, Monitor monitor)
        {
            if (visual.DisplayRectangle.Right > monitor.Width)
            {
                visual.Left = Math.Max(0d, monitor.Width - visual.DisplayRectangle.Width);
            }

            if (visual.DisplayRectangle.Bottom > monitor.Height)
            {
                visual.Top = Math.Max(0d, monitor.Height - visual.DisplayRectangle.Height);
            }
        }

        private void TransformControl(HeliosVisual visual, double scale, Point translation)
        {
            visual.Left = visual.Left * scale + translation.X;
            visual.Top = visual.Top * scale + translation.Y;
            visual.Width = Math.Max(visual.Width * scale, 1d);
            visual.Height = Math.Max(visual.Height * scale, 1d);

            // child coordinates are relative, so we don't have to translate
            visual.ScaleChildren(scale, scale);
        }
    }
}
