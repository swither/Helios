using System;
using System.Collections.Generic;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public class ShadowMonitor : ShadowVisual
    {
        protected MonitorViewModel _monitorViewModel;
        private int _viewportCount;

        public string Key { get; private set; }
        internal ShadowMonitor(IShadowVisualParent parent, Monitor monitor)
            : base(parent, monitor, monitor, false)
        {
            Key = CreateKey(monitor);
            _monitorViewModel = new MonitorViewModel(Key, monitor, parent.GlobalOffset, parent.Scale);
            _monitorViewModel.Enabled += MonitorViewModel_Enabled;
            _monitorViewModel.Disabled += MonitorViewModel_Disabled;
            _monitorViewModel.AddedToMain += MonitorViewModel_AddedToMain;
            _monitorViewModel.RemovedFromMain += MonitorViewModel_RemovedFromMain;
            _monitorViewModel.AddedToUserInterface += MonitorViewModel_AddedToUserInterface;
            _monitorViewModel.RemovedFromUserInterface += MonitorViewModel_RemovedFromUserInterface;
        }

        /// <summary>
        /// deferred initialization so our factory can index this before we add children to it
        /// </summary>
        internal void Instrument()
        {
            Instrument(Monitor, Monitor);
        }

        public static string CreateKey(Monitor display)
        {
            // the position and size of a monitor is all we care about
            return $"{display.Left}_{display.Top}_{display.Width}_{display.Height}";
        }

        // XXX when this is all done, collapse all these into one if none of them have any code

        private void MonitorViewModel_Disabled(object sender, EventArgs e)
        {
            // change of include/exclude setting, not actually a property but rather
            // a local SettingsManager setting, but we treat it like an update to the monitor object
            // because it will change geometry
            _parent.ChangeMonitor(this);
        }

        private void MonitorViewModel_Enabled(object sender, EventArgs e)
        {
            // change of include/exclude setting, not actually a property but rather
            // a local SettingsManager setting, but we treat it like an update to the monitor object
            // because it will change geometry
            _parent.ChangeMonitor(this);
        }

        private void MonitorViewModel_AddedToMain(object sender, EventArgs e)
        {
            // change of monitor being part of main view, not actually a property but rather
            // a local SettingsManager setting, but we treat it like an update to the monitor object
            // because it will change geometry
            _parent.ChangeMonitor(this);
        }

        private void MonitorViewModel_RemovedFromMain(object sender, EventArgs e)
        {
            // change of monitor being part of main view, not actually a property but rather
            // a local SettingsManager setting, but we treat it like an update to the monitor object
            // because it will change geometry
            _parent.ChangeMonitor(this);
        }

        private void MonitorViewModel_AddedToUserInterface(object sender, EventArgs e)
        {
            // change of monitor being part of UI view, not actually a property but rather
            // a local SettingsManager setting, but we treat it like an update to the monitor object
            // because it will change geometry
            _parent.ChangeMonitor(this);
        }

        private void MonitorViewModel_RemovedFromUserInterface(object sender, EventArgs e)
        {
            // change of monitor being part of UI view, not actually a property but rather
            // a local SettingsManager setting, but we treat it like an update to the monitor object
            // because it will change geometry
            _parent.ChangeMonitor(this);
        }

        protected override void OnModified()
        {
            base.OnModified();

            // we may have changed enough to where our key doesn't match (during monitor reset)
            // and so we need to reindex in that case and load settings again
            string newKey = CreateKey(_monitor);
            if (newKey != Key)
            {
                string oldKey = Key;
                Key = newKey;
                _monitorViewModel.ChangeKey(Key);
                _parent.ChangeMonitorKey(this, oldKey, newKey);
            }

            // need to update all viewports on this monitor
            foreach (ShadowVisual child in _children.Values)
            {
                UpdateChild(child);
            }
            _monitorViewModel.Update(_monitor);
            _parent.ChangeMonitor(this);
        }

        private void UpdateChild(ShadowVisual node)
        {
            if (node.IsViewport)
            {
                node.ViewportViewModel.Update(_monitor);
            }
            foreach (ShadowVisual child in node.Children.Values)
            {
                UpdateChild(child);
            }
        }

        internal override void Dispose()
        {
            base.Dispose();
            _monitorViewModel.Enabled -= MonitorViewModel_Enabled;
            _monitorViewModel.Disabled -= MonitorViewModel_Disabled;
            _monitorViewModel.AddedToMain -= MonitorViewModel_AddedToMain;
            _monitorViewModel.RemovedFromMain -= MonitorViewModel_RemovedFromMain;
            _monitorViewModel.AddedToUserInterface -= MonitorViewModel_AddedToUserInterface;
            _monitorViewModel.RemovedFromUserInterface -= MonitorViewModel_RemovedFromUserInterface;
        }

        // XXX this is all wrong.  all these properties that are relevant to calculating the output should be in ShadowMonitor (the model)
        // XXX instead.  remove access to the view model in MonitorSetup.cs and refactor until everything is in the right place
        public MonitorViewModel MonitorViewModel => _monitorViewModel;

        internal static IEnumerable<string> GetAllKeys(Monitor monitor)
        {
            string baseKey = CreateKey(monitor);
            return MonitorViewModel.GetAllKeys(baseKey);
        }

        internal bool AddViewport()
        {
            _viewportCount++;
            if (_viewportCount == 1)
            {
                _monitorViewModel.HasViewports = true;
                return true;
            }
            return false;
        }

        internal bool RemoveViewport()
        {
            _viewportCount--;
            if (_viewportCount == 0)
            {
                _monitorViewModel.HasViewports = false;
                return true;
            }
            return false;
        }

        internal int ViewportCount => _viewportCount;
    }
}
