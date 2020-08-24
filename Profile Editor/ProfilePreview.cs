//  Copyright 2014 Craig Courtney
//  Copyright 2020 Helios Contributors

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

using System.Linq;

namespace GadrocsWorkshop.Helios.ProfileEditor
{
    using GadrocsWorkshop.Helios.Windows.Controls;
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;

    public class ProfilePreview : FrameworkElement
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private class MonitorRectangle
        {
            public Rect DisplayRectangle;
            public Monitor Monitor;
            public HeliosVisualView View;
            public MonitorAdorner Adorner;
        }

        private readonly VisualCollection _children;
        private bool _setAdorners = false;

        private readonly List<MonitorRectangle> _monitorRectangles = new List<MonitorRectangle>();

        private readonly DrawingBrush _checkeredBrush;
        private readonly Pen _borderPen;

        static ProfilePreview()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProfilePreview), new FrameworkPropertyMetadata(typeof(ProfilePreview)));
        }

        public ProfilePreview()
        {
            _children = new VisualCollection(this);

            DrawingGroup checkerGroup = new DrawingGroup();
            checkerGroup.Children.Add(new GeometryDrawing(Brushes.White, null, new RectangleGeometry(new Rect(0, 0, 100, 100))));

            DrawingGroup grayGroup = new DrawingGroup();
            grayGroup.Children.Add(new GeometryDrawing(Brushes.LightGray, null, new RectangleGeometry(new Rect(0, 0, 50, 50))));
            grayGroup.Children.Add(new GeometryDrawing(Brushes.LightGray, null, new RectangleGeometry(new Rect(50, 50, 50, 50))));

            checkerGroup.Children.Add(grayGroup);
            checkerGroup.Freeze();

            _checkeredBrush = new DrawingBrush(checkerGroup)
            {
                Viewport = new Rect(0, 0, 10, 10),
                ViewportUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.Tile
            };
            _checkeredBrush.Freeze();

            _borderPen = new Pen(Brushes.Black, 1d);

            Focusable = false;
        }

        #region Properties

        public HeliosProfile Profile
        {
            get => (HeliosProfile)GetValue(ProfileProperty);
            set => SetValue(ProfileProperty, value);
        }

        public static readonly DependencyProperty ProfileProperty =
            DependencyProperty.Register("Profile", typeof(HeliosProfile), typeof(ProfilePreview), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public bool ShowPanels
        {
            get => (bool)GetValue(ShowPanelsProperty);
            set => SetValue(ShowPanelsProperty, value);
        }

        public static readonly DependencyProperty ShowPanelsProperty =
            DependencyProperty.Register("ShowPanels", typeof(bool), typeof(ProfilePreview), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.None));

        public double ZoomFactor
        {
            get => (double)GetValue(ZoomFactorProperty);
            set => SetValue(ZoomFactorProperty, value);
        }

        public static readonly DependencyProperty ZoomFactorProperty =
            DependencyProperty.Register("ZoomFactor", typeof(double), typeof(ProfilePreview), new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion

        #region Visual Methods

        protected override int VisualChildrenCount => _children.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }

        #endregion

        protected override Size MeasureOverride(Size availableSize)
        {
            Size resultSize = new Size(0, 0);

            if (Profile == null)
            {
                return resultSize;
            }

            Profile.Monitors.UpdateVirtualScreen();

            resultSize.Width = double.IsPositiveInfinity(availableSize.Width) ? Math.Max(1d, Profile.Monitors.VirtualScreenWidth * ZoomFactor) : availableSize.Width;
            resultSize.Height = double.IsPositiveInfinity(availableSize.Height) ? Math.Max(1d, Profile.Monitors.VirtualScreenHeight * ZoomFactor) : availableSize.Height;

            double scale = Math.Min(resultSize.Height / Profile.Monitors.VirtualScreenHeight, resultSize.Width / Profile.Monitors.VirtualScreenWidth);

            resultSize.Width = Profile.Monitors.VirtualScreenWidth * scale;
            resultSize.Height = Profile.Monitors.VirtualScreenHeight * scale;

            foreach (HeliosVisualView child in _children.OfType<HeliosVisualView>())
            {
                Size childSize = new Size(child.Visual.Width * scale,
                    child.Visual.Height * scale);
                child.Measure(childSize);
            }

            return resultSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Profile == null)
            {
                return finalSize;
            }

            // NOTE: there should not be any code paths right now where we did not just 
            // update this in MeasureOverride
            Profile.Monitors.UpdateVirtualScreen();

            double scale = Math.Min(finalSize.Width / Profile.Monitors.VirtualScreenWidth, finalSize.Height / Profile.Monitors.VirtualScreenHeight);

            foreach (MonitorRectangle monitorRectangle in _monitorRectangles)
            {
                monitorRectangle.DisplayRectangle = new Rect(((monitorRectangle.Monitor.Left - Profile.Monitors.VirtualScreenLeft) * scale),
                    ((monitorRectangle.Monitor.Top - Profile.Monitors.VirtualScreenTop) * scale),
                    monitorRectangle.Monitor.Width * scale,
                    monitorRectangle.Monitor.Height * scale);
                monitorRectangle.View.Arrange(monitorRectangle.DisplayRectangle);
            }

            return finalSize;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ProfileProperty)
            {
                if (e.OldValue is HeliosProfile oldProfile)
                {
                    oldProfile.Monitors.CollectionChanged -= Monitors_CollectionChanged;
                }

                if (Profile != null)
                {
                    Profile.Monitors.CollectionChanged += Monitors_CollectionChanged;
                }

                Dispatcher.Invoke(UpdateMonitors);
            }
            else if (e.Property == ShowPanelsProperty)
            {
                foreach (HeliosVisualView panelView in _children.OfType<HeliosVisualView>())
                {
                    panelView.Visibility = ShowPanels ? Visibility.Visible : Visibility.Hidden;
                }
            }

            base.OnPropertyChanged(e);
        }

        private void Monitors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (Monitor monitor in e.OldItems)
                {
                    monitor.Resized -= Monitor_MoveResize;
                    monitor.Moved -= Monitor_MoveResize;
                }
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (Monitor monitor in e.NewItems)
                {
                    monitor.Resized += Monitor_MoveResize;
                    monitor.Moved += Monitor_MoveResize;
                }
            }

            Dispatcher.BeginInvoke(new Action(UpdateMonitors));
        }

        private void Monitor_MoveResize(object sender, EventArgs e)
        {
            if (CheckAccess())
            {
                InvalidateAll();
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(InvalidateAll));
            }
        }

        private void InvalidateAll()
        {
            InvalidateMeasure();
            InvalidateArrange();
            InvalidateVisual();
        }

        private void UpdateMonitors()
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
            foreach (MonitorRectangle monitorRectangle in _monitorRectangles)
            {
                layer?.Remove(monitorRectangle.Adorner);
                _children.Remove(monitorRectangle.View);
            }

            _monitorRectangles.Clear();

            if (Profile != null)
            {
                _setAdorners = true;

                int i = 1;
                foreach (Monitor monitor in Profile.Monitors)
                {
                    HeliosVisualView monitorView = new HeliosVisualView
                    {
                        Visual = monitor, Visibility = ShowPanels ? Visibility.Visible : Visibility.Hidden
                    };
                    _children.Add(monitorView);

                    _monitorRectangles.Add(new MonitorRectangle
                    {
                        Monitor = monitor, 
                        View = monitorView, 
                        Adorner = new MonitorAdorner(monitorView, i++.ToString(), monitor)
                    });
                }
            }

            // rebuild window and render
            InvalidateMeasure();
            InvalidateArrange();
            InvalidateVisual();
        }

        private void SetAdorners()
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
            if (layer != null)
            {
                foreach (MonitorRectangle monitorRectangle in _monitorRectangles)
                {
                    layer.Add(monitorRectangle.Adorner);
                }
            }
            _setAdorners = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_setAdorners)
            {
                SetAdorners();
            }

            foreach (MonitorRectangle monitorRectangle in _monitorRectangles)
            {
                drawingContext.DrawRectangle(_checkeredBrush, _borderPen, monitorRectangle.DisplayRectangle);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2)
            {
                return;
            }

            Point position = e.GetPosition(this);

            // dispatch OpenProfileItem command to selected monitor object
            foreach (MonitorRectangle monitorRectangle in _monitorRectangles.Where(t => t.DisplayRectangle.Contains(position)))
            {
                if (ProfileEditorCommands.OpenProfileItem.CanExecute(monitorRectangle.Monitor, this))
                {
                    ProfileEditorCommands.OpenProfileItem.Execute(monitorRectangle.Monitor, this);
                }

                // consider only first display rect that is hit, because monitors don't overlap
                break;
            }

            // either way, no other clicking allowed
            e.Handled = true;
        }
        
    }
}
