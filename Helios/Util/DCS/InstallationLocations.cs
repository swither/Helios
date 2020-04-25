using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace GadrocsWorkshop.Helios.Util.DCS
{
    public class InstallationLocations : DependencyObject
    {
        public event EventHandler<LocationEvent> Added;
        public event EventHandler<LocationEvent> Disabled;
        public event EventHandler<LocationEvent> Enabled;
        public event EventHandler<LocationEvent> Removed;

        private static InstallationLocations _singleton;

        public static InstallationLocations Singleton => _singleton ?? (_singleton = new InstallationLocations());

        public class LocationEvent : EventArgs
        {
            internal LocationEvent(InstallationLocation location)
            {
                Location = location;
            }

            #region Properties

            public InstallationLocation Location { get; }

            #endregion
        }

        private InstallationLocations()
        {
            LoadAll();
            if (ConfigManager.SettingsManager is ISettingsManager2 settings)
            {
                settings.Synchronized += Settings_Synchronized;
            }
        }

        private void Settings_Synchronized(object sender, EventArgs e)
        {
            IList<InstallationLocation> removed = Items.ToList();
            Items.Clear();
            foreach (InstallationLocation location in removed)
            {
                Removed?.Invoke(this, new LocationEvent(location));
            }
            LoadAll();
        }

        private void LoadAll()
        {
            // load from settings XML
            foreach (InstallationLocation item in InstallationLocation.ReadSettings())
            {
                DoAdd(item);
            }
        }

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
                InstallationLocation location = (InstallationLocation) sender;
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

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<InstallationLocation>),
                typeof(InstallationLocations), new PropertyMetadata(new ObservableCollection<InstallationLocation>()));

        #region Properties

        public IList<InstallationLocation> Active => Items.Where(l => l.IsEnabled).ToList();

        public ObservableCollection<InstallationLocation> Items
        {
            get => (ObservableCollection<InstallationLocation>) GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        #endregion
    }
}