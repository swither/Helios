using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace GadrocsWorkshop.Helios.Util.DCS
{
    public class InstallationLocations : DependencyObject
    {
        private static InstallationLocations _singleton;

        public class LocationEvent: EventArgs 
        {
            internal LocationEvent(InstallationLocation location)
            {
                Location = location;
            }

            public InstallationLocation Location { get; private set; }
        }

        public event EventHandler<LocationEvent> Added;
        public event EventHandler<LocationEvent> Removed;
        public event EventHandler<LocationEvent> Enabled;
        public event EventHandler<LocationEvent> Disabled;

        public static InstallationLocations Singleton
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new InstallationLocations();
                }
                return _singleton;
            }
        }

        private InstallationLocations()
        {
            // load from settings XML
            foreach (InstallationLocation item in InstallationLocation.ReadSettings())
            {
                DoAdd(item);
            }
        }

        public ObservableCollection<InstallationLocation> Items
        {
            get { return (ObservableCollection<InstallationLocation>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<InstallationLocation>), typeof(InstallationLocations), new PropertyMetadata(new ObservableCollection<InstallationLocation>()));

        internal bool TryAdd(InstallationLocation newItem)
        {
            // scan list; O(n) but this is a UI action
            foreach (InstallationLocation existing in Items)
            {
                if (existing.Path.Equals(newItem.Path, StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }
            }
            
            // add to our collection and register handlers
            DoAdd(newItem);
            
            // write it to settings file
            newItem.UpdateSettings();

            // notify our customers
            Added?.Invoke(this, new LocationEvent(newItem));
            return true;
        }

        private void DoAdd(InstallationLocation newItem)
        {
            newItem.ChangeEnabled += (sender, e) =>
            {
                InstallationLocation location = (InstallationLocation)sender;
                if (location.IsEnabled)
                {
                    Enabled?.Invoke(this, new LocationEvent(location));
                }
                else
                {
                    Disabled?.Invoke(this, new LocationEvent(location));
                }
            };
            Items.Add(newItem);
        }

        internal bool TryRemove(InstallationLocation oldItem)
        {
            if (oldItem == null)
            {
                return false;
            }
            if (!Items.Remove(oldItem))
            {
                return false;
            }
            oldItem.DeleteSettings();
            Removed?.Invoke(this, new LocationEvent(oldItem));
            return true;
        }
    }
}