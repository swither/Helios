using System;
using System.Collections.Generic;
using System.Windows;
using GadrocsWorkshop.Helios.Windows;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    // XXX move almost all of this into ShadowMonitor, except anythign that is presentation only and not persisted,
    public class MonitorViewModel: HeliosViewModel<ShadowMonitor>
    {
        private double _scale;
        private Vector _globalOffset;
        private string _canExcludeNarrative;

        internal MonitorViewModel(ShadowMonitor monitor, Vector globalOffset, double scale)
        : base(monitor)
        {
            _scale = scale;
            _globalOffset = globalOffset;
            Update(monitor.Monitor);

            // changes to the Helios monitor geometry
            monitor.MonitorChanged += Monitor_MonitorChanged;
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

        internal static IEnumerable<string> GetAllKeys(string baseKey)
        {
            yield return baseKey;
            yield return $"{baseKey}_Main";
            yield return $"{baseKey}_UI";
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
            ConfigManager.LogManager.LogDebug($"scaled monitor view {this.GetHashCode()} for monitor setup UI is {Rect}");
        }

        private void UpdateIncludedNarrative()
        {
            if (Data.Included)
            {
                IncludedNarrative = _canExcludeNarrative;
            }
            else
            {
                IncludedNarrative =
                    "This monitor is not being drawn on by DCS because it is excluded by this selection.";
            }
        }

        internal void SetCanExclude(bool value, string narrative)
        {
            CanBeExcluded = value;
            _canExcludeNarrative = narrative;
            if ((!Data.Included) && (!value))
            {
                Data.Included = true;
            }
            UpdateIncludedNarrative();
        }

        public bool CanBeExcluded
        {
            get { return (bool)GetValue(CanBeExcludedProperty); }
            set { SetValue(CanBeExcludedProperty, value); }
        }
        public static readonly DependencyProperty CanBeExcludedProperty =
            DependencyProperty.Register("CanBeExcluded", typeof(bool), typeof(MonitorViewModel), new PropertyMetadata(false));

        public bool CanBeRemovedFromMain
        {
            get { return (bool)GetValue(CanBeRemovedFromMainProperty); }
            set { SetValue(CanBeRemovedFromMainProperty, value); }
        }
        public static readonly DependencyProperty CanBeRemovedFromMainProperty =
            DependencyProperty.Register("CanBeRemovedFromMain", typeof(bool), typeof(MonitorViewModel), new PropertyMetadata(true));

        public bool CanBeRemovedFromUserInterface
        {
            get { return (bool)GetValue(CanBeRemovedFromUserInterfaceProperty); }
            set { SetValue(CanBeRemovedFromUserInterfaceProperty, value); }
        }
        public static readonly DependencyProperty CanBeRemovedFromUserInterfaceProperty =
            DependencyProperty.Register("CanBeRemovedFromUserInterface", typeof(bool), typeof(MonitorViewModel), new PropertyMetadata(true));

        public Rect Rect
        {
            get { return (Rect)GetValue(RectProperty); }
            set { SetValue(RectProperty, value); }
        }
        public static readonly DependencyProperty RectProperty =
            DependencyProperty.Register("Rect", typeof(Rect), typeof(MonitorViewModel), new PropertyMetadata(null));

        public string IncludedNarrative
        {
            get { return (string)GetValue(IncludedNarrativeProperty); }
            set { SetValue(IncludedNarrativeProperty, value); }
        }
        public static readonly DependencyProperty IncludedNarrativeProperty =
            DependencyProperty.Register("IncludedNarrative", typeof(string), typeof(MonitorViewModel), new PropertyMetadata(null));

        public Rect RawRect { get; set; }
        public bool HasContent => Data.Main || Data.UserInterface || (Data.ViewportCount > 0);
    }
}
