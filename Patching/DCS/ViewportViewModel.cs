using System;
using System.Windows;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public class ViewportViewModel: DependencyObject
    {
        private Vector _globalOffset;
        private double _scale;
        private Rect _monitor = new Rect(0, 0, 1, 1);
        private Rect _viewport = new Rect(0, 0, 1, 1);

        public ViewportViewModel(Monitor monitor, Vector globalOffset, HeliosVisual viewport, double scale)
        {
            _globalOffset = globalOffset;
            _scale = scale;
            Update(monitor);
            Update(viewport);
        }

        internal void Update(Monitor monitor)
        {
            _monitor.X = monitor.Left;
            _monitor.Y = monitor.Top;
            _monitor.Width = monitor.Width;
            _monitor.Height = monitor.Height;
            Update();
        }

        internal void Update(Vector globalOffset)
        {
            _globalOffset = globalOffset;
            Update();
        }

        internal void Update(HeliosVisual viewport)
        {
            _viewport.X = viewport.Left;
            _viewport.Y = viewport.Top;
            _viewport.Width = viewport.Width;
            _viewport.Height = viewport.Height;
            Update();
        }

        internal void Update(double scale)
        {
            _scale = scale;
            Update();
        }

        private void Update()
        {
            Rect absolute = _viewport;
            absolute.Offset(_monitor.X, _monitor.Y);
            absolute.Offset(_globalOffset);
            absolute.Scale(_scale, _scale);
            Rect = absolute;
            ConfigManager.LogManager.LogDebug($"scaled viewport view {this.GetHashCode()} for monitor setup UI is {Rect}");
        }

        public Rect Rect
        {
            get { return (Rect)GetValue(RectProperty); }
            set { SetValue(RectProperty, value); }
        }
        public static readonly DependencyProperty RectProperty =
            DependencyProperty.Register("Rect", typeof(Rect), typeof(ViewportViewModel), new PropertyMetadata(null));
    }
}