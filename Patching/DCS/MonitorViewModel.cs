using System.Collections.Generic;
using System.Windows;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public class MonitorViewModel: DependencyObject
    {
        public event System.EventHandler Enabled;
        public event System.EventHandler Disabled;
        public event System.EventHandler AddedToMain;
        public event System.EventHandler RemovedFromMain;
        public event System.EventHandler AddedToUserInterface;
        public event System.EventHandler RemovedFromUserInterface;

        private string _key;
        private double _scale;
        private Vector _globalOffset;

        public MonitorViewModel(string key, Monitor monitor, Vector globalOffset, double scale)
        {
            _key = key;
            LoadSettings();
            _scale = scale;
            _globalOffset = globalOffset;
            Update(monitor);
        }

        private void LoadSettings()
        {
            Included = ConfigManager.SettingsManager.LoadSetting(MonitorSetup.SETTINGS_GROUP, _key, true);
            Main = ConfigManager.SettingsManager.LoadSetting(MonitorSetup.SETTINGS_GROUP, $"{_key}_Main", false);
            Main = ConfigManager.SettingsManager.LoadSetting(MonitorSetup.SETTINGS_GROUP, $"{_key}_UI", false);
        }

        private void DeleteSettings()
        {
            ISettingsManager2 settings2 = ConfigManager.SettingsManager as ISettingsManager2;
            settings2.DeleteSetting(MonitorSetup.SETTINGS_GROUP, _key);
            settings2.DeleteSetting(MonitorSetup.SETTINGS_GROUP, $"{_key}_Main");
            settings2.DeleteSetting(MonitorSetup.SETTINGS_GROUP, $"{_key}_UI");
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

        public bool HasContent => Main || UserInterface || HasViewports;

        public bool Included
        {
            get { return (bool)GetValue(IncludedProperty); }
            set {
                SetValue(IncludedProperty, value);
            }
        }
        public static readonly DependencyProperty IncludedProperty =
            DependencyProperty.Register("Included", typeof(bool), typeof(MonitorViewModel), new PropertyMetadata(true, OnIncludedChanged));
        private static void OnIncludedChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            if ((bool)args.NewValue)
            {
                (ConfigManager.SettingsManager as ISettingsManager2).DeleteSetting(MonitorSetup.SETTINGS_GROUP, ((MonitorViewModel)target)._key);
                ((MonitorViewModel)target).Enabled?.Invoke(target, System.EventArgs.Empty);
            }
            else
            {
                ConfigManager.SettingsManager.SaveSetting(MonitorSetup.SETTINGS_GROUP, ((MonitorViewModel)target)._key, false);
                ((MonitorViewModel)target).Disabled?.Invoke(target, System.EventArgs.Empty);
            }
        }

        public bool CanBeExcluded
        {
            get { return (bool)GetValue(CanBeExcludedProperty); }
            set { SetValue(CanBeExcludedProperty, value); }
        }
        public static readonly DependencyProperty CanBeExcludedProperty =
            DependencyProperty.Register("CanBeExcluded", typeof(bool), typeof(MonitorViewModel), new PropertyMetadata(false));

        /// <summary>
        /// changes the key used for load and save settings and switches settings to those of 
        /// the new key
        /// 
        /// settings for the old key are deleted
        /// </summary>
        /// <param name="key"></param>
        internal void ChangeKey(string key)
        {
            if (key == _key)
            {
                return;
            }
            DeleteSettings();
            _key = key;
            LoadSettings();
        }

        public bool Main
        {
            get { return (bool)GetValue(MainProperty); }
            set
            {
                SetValue(MainProperty, value);
            }
        }
        public static readonly DependencyProperty MainProperty =
            DependencyProperty.Register("Main", typeof(bool), typeof(MonitorViewModel), new PropertyMetadata(true, OnMainChanged));
        private static void OnMainChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            string key = $"{((MonitorViewModel)target)._key}_Main";
            MonitorViewModel model = ((MonitorViewModel)target);
            if ((bool)args.NewValue)
            {
                ConfigManager.SettingsManager.SaveSetting(MonitorSetup.SETTINGS_GROUP, key, true);
                model.AddedToMain?.Invoke(target, System.EventArgs.Empty);
            }
            else
            {
                ConfigManager.SettingsManager.SaveSetting(MonitorSetup.SETTINGS_GROUP, key, false);
                model.RemovedFromMain?.Invoke(target, System.EventArgs.Empty);
            }
        }

        public bool CanBeRemovedFromMain
        {
            get { return (bool)GetValue(CanBeRemovedFromMainProperty); }
            set { SetValue(CanBeRemovedFromMainProperty, value); }
        }
        public static readonly DependencyProperty CanBeRemovedFromMainProperty =
            DependencyProperty.Register("CanBeRemovedFromMain", typeof(bool), typeof(MonitorViewModel), new PropertyMetadata(true));

        public bool UserInterface
        {
            get { return (bool)GetValue(UserInterfaceProperty); }
            set
            {
                SetValue(UserInterfaceProperty, value);
            }
        }
        public static readonly DependencyProperty UserInterfaceProperty =
             DependencyProperty.Register("UserInterface", typeof(bool), typeof(MonitorViewModel), new PropertyMetadata(false, OnUserInterfaceChanged));
        private static void OnUserInterfaceChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            string key = $"{((MonitorViewModel)target)._key}_UserInterface";
            MonitorViewModel model = ((MonitorViewModel)target);
            if ((bool)args.NewValue)
            {
                ConfigManager.SettingsManager.SaveSetting(MonitorSetup.SETTINGS_GROUP, key, true);
                model.AddedToUserInterface?.Invoke(target, System.EventArgs.Empty);
            }
            else
            {
                ConfigManager.SettingsManager.SaveSetting(MonitorSetup.SETTINGS_GROUP, key, false);
                model.RemovedFromUserInterface?.Invoke(target, System.EventArgs.Empty);
            }
        }

        public bool CanBeRemovedFromUserInterface
        {
            get { return (bool)GetValue(CanBeRemovedFromUserInterfaceProperty); }
            set { SetValue(CanBeRemovedFromUserInterfaceProperty, value); }
        }
        public static readonly DependencyProperty CanBeRemovedFromUserInterfaceProperty =
            DependencyProperty.Register("CanBeRemovedFromUserInterface", typeof(bool), typeof(MonitorViewModel), new PropertyMetadata(true));

        public bool HasViewports
        {
            get { return (bool)GetValue(HasViewportsProperty); }
            set { SetValue(HasViewportsProperty, value); }
        }
        public static readonly DependencyProperty HasViewportsProperty =
            DependencyProperty.Register("HasViewports", typeof(bool), typeof(MonitorViewModel), new PropertyMetadata(false));

        public Rect Rect
        {
            get { return (Rect)GetValue(RectProperty); }
            set { SetValue(RectProperty, value); }
        }
        public static readonly DependencyProperty RectProperty =
            DependencyProperty.Register("Rect", typeof(Rect), typeof(MonitorViewModel), new PropertyMetadata(null));

        public Rect RawRect { get; set; }
    }
}
