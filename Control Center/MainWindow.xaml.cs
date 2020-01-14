//  Copyright 2014 Craig Courtney
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

namespace GadrocsWorkshop.Helios.ControlCenter
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Threading;
    using GadrocsWorkshop.Helios.Splash;
    using GadrocsWorkshop.Helios.ProfileAwareInterface;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const long TOPMOST_TICK_COUNT = 3 * TimeSpan.TicksPerSecond;

        private List<string> _profiles = new List<string>();
        private int _profileIndex = -1;

        private DispatcherTimer _dispatcherTimer;
        private List<MonitorWindow> _windows = new List<MonitorWindow>();
        private bool _deletingProfile = false;
        private long _lastTick = 0;

        private WindowInteropHelper _helper;
        private bool _prefsShown = false;

        private HotKey _hotkey = null;

        // current status message to display
        private StatusValue _status = StatusValue.Empty;

        // most recently received simulator info
        private string _lastProfileHint = "";
        private string _lastDriverStatus = "";

        public MainWindow()
        {
            InitializeComponent();

            ConfigManager.LogManager.LogInfo("Initializing Main Window");
            KeyboardEmulator.ControlCenterSession = true;  // Keyboard emulator needs to know that we're in the Control Center

            displaySplash(4000);  // Display a dynamic splash panel with release and credits

            MinimizeCheckBox.IsChecked = ConfigManager.SettingsManager.LoadSetting("ControlCenter", "StartMinimized", false);
            if (MinimizeCheckBox.IsChecked == true)
            {
                Minimize();
            }
            else
            {
                Maximize();
            }

            // if true, Control Center will automatically start the same profile as is being run by the export script
            ProfileAutoStartCheckBox.IsChecked = ConfigManager.SettingsManager.LoadSetting("ControlCenter", "ProfileAutoStart", true);

            LoadProfileList(ConfigManager.SettingsManager.LoadSetting("ControlCenter", "LastProfile", ""));
            if (_profileIndex == -1 && _profiles.Count > 0)
            {
                _profileIndex = 0;
            }
            if (_profileIndex > -1 && _profileIndex < _profiles.Count)
            {
                SelectedProfileName = System.IO.Path.GetFileNameWithoutExtension(_profiles[_profileIndex]);
            }
            else
            {
                SelectedProfileName = "- No Profiles Available -";
            }

            if (ConfigManager.SettingsManager.IsSettingAvailable("ControlCenter", "TouchScreenMouseSuppressionPeriod"))
            {
                TouchScreenDelaySlider.Value = ConfigManager.SettingsManager.LoadSetting("ControlCenter", "TouchScreenMouseSuppressionPeriod", 0);
                if (TouchScreenDelaySlider.Value > 0)
                {
                    TouchscreenDelayTextBlock.Visibility = Visibility.Visible;
                    TouchScreenDelaySlider.Visibility = Visibility.Visible;
                    TouchScreenDelayBorder.Visibility = Visibility.Visible;
                    TouchScreenDelayTitle.Visibility = Visibility.Visible;
                    TouchscreenCheckBox.IsChecked = true;
                }
                else
                {
                    TouchscreenCheckBox.IsChecked = false;
                }
            }

            try
            {
                RegistryKey pathKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                using (pathKey)
                {
                    if (pathKey.GetValue("Helios") != null)
                    {
                        AutoStartCheckBox.IsChecked = true;
                    }
                    pathKey.Close();
                }
            }
            catch (Exception e)
            {
                AutoStartCheckBox.IsChecked = false;
                AutoStartCheckBox.IsEnabled = false;
                AutoStartCheckBox.ToolTip = "Unable to read/write registry for auto start.  Try running Control Center as an administrator.";
                ConfigManager.LogManager.LogError("Error checking for auto start.", e);
            }

            AutoHideCheckBox.IsChecked = ConfigManager.SettingsManager.LoadSetting("ControlCenter", "AutoHide", false);
            StatusMessage = StatusValue.RunningVersion;
        }

        private void displaySplash(Int32 splashDelay)
        {
            About aboutDialog = new About();
            aboutDialog.InitializeComponent();
            aboutDialog.Show();
            System.Threading.Thread.Sleep(splashDelay);
            aboutDialog.Close();
        }

        #region Properties

        public HeliosProfile ActiveProfile
        {
            get { return (HeliosProfile)GetValue(ActiveProfileProperty); }
            set { SetValue(ActiveProfileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActiveProfile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveProfileProperty =
            DependencyProperty.Register("ActiveProfile", typeof(HeliosProfile), typeof(MainWindow), new UIPropertyMetadata(null));

        public string SelectedProfileName
        {
            get { return (string)GetValue(SelectedProfileNameProperty); }
            set { SetValue(SelectedProfileNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedProfileName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedProfileNameProperty =
            DependencyProperty.Register("SelectedProfileName", typeof(string), typeof(MainWindow), new PropertyMetadata(""));

        public enum StatusValue
        {
            Empty,
            License, // Helios used to display a license; this case is here in case we need to put that back
            DeleteWarning,
            Running,
            Loading,
            LoadError,
            RunningVersion,
            ProfileVersionHigher,
            BadMonitorConfig
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(MainWindow), new PropertyMetadata(""));

        public StatusValue StatusMessage
        {
            get { return _status; }
            set {
                _status = value;
                UpdateStatusMessage();
            }
        }

        private void ComposeStatusMessage(string firstLine)
        {
            string message = firstLine;
            if (_lastProfileHint.Length > 0)
            {
                message += $"\nSimulator is {_lastProfileHint}";
            } else {
                message += "\n";
            }
            if (_lastDriverStatus.Length > 0)
            {
                message += $"\nSimulator is running {_lastDriverStatus}";
            }
            SetValue(MessageProperty, message);
        }        

        private void UpdateStatusMessage()
        {
            // centralize all these messages, because they all need to fit in the same space
            switch(_status)
            {
                case StatusValue.Empty:
                    // fall through
                case StatusValue.License:
                    ComposeStatusMessage("");
                    break;
                case StatusValue.DeleteWarning:
                    // this multiline message does not combine with anything
                    SetValue(MessageProperty, "!!WARNING!!\nYou are about to permanetly delete this profile.  Please press start to confirm.");
                    break;
                case StatusValue.Running:
                    ComposeStatusMessage("Running Profile");
                    break;
                case StatusValue.Loading:
                    ComposeStatusMessage("Loading Profile...");
                    break;
                case StatusValue.LoadError:
                    ComposeStatusMessage("Error loading Profile");
                    break;
                case StatusValue.RunningVersion:
                    Version ver = Assembly.GetEntryAssembly().GetName().Version;
                    ComposeStatusMessage($"{ver.Major.ToString()}.{ver.Minor.ToString()}.{ver.Build.ToString("0000")}.{ver.Revision.ToString("0000")}\nProject Fork: BlueFinBima\n");
                    break;
                case StatusValue.ProfileVersionHigher:
                    // this multiline message does not combine with anything
                    SetValue(MessageProperty, "Cannot display this profile because it was created with a newer version of Helios.  Please upgrade to the latest version.");
                    break;
                case StatusValue.BadMonitorConfig:
                    // this multiline message does not combine with anything
                    SetValue(MessageProperty, "Cannot display this profile because it has an invalid monitor configuration.  Please open the editor and select reset monitors from the profile menu.");
                    break;
            }
        }

        public string HotKeyDescription
        {
            get { return (string)GetValue(HotKeyDescriptionProperty); }
            set { SetValue(HotKeyDescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HotKeyDescription.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HotKeyDescriptionProperty =
            DependencyProperty.Register("HotKeyDescription", typeof(string), typeof(MainWindow), new PropertyMetadata(""));


        #endregion

        private void Minimize()
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        internal void Maximize()
        {
            PowerButton.IsChecked = true;
            WindowState = System.Windows.WindowState.Normal;
            if (_helper != null)
            {
                NativeMethods.BringWindowToTop(_helper.Handle);
            }
        }

        #region Commands

        private void RunProfile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _deletingProfile = false;

            string profileToLoad = e.Parameter as string;
            if (profileToLoad == null || !File.Exists(profileToLoad))
            {
                NativeMethods.BringWindowToTop(_helper.Handle);
            }
            else
            {
                if (ActiveProfile != null)
                {
                    if (ActiveProfile.IsStarted)
                    {
                        ActiveProfile.Stop();
                    }
                    ActiveProfile = null;
                }

                // try to load the profile, setting SelectedProfileName in the process
                LoadProfile(profileToLoad);

                if (ActiveProfile != null)
                {
                    // we need to fix up the selection index or Next/Prev buttons will not work correctly
                    // because LoadProfile sets SelectedProfileName but assumes the prev/next has already been set
                    // by user interaction.  however, we selected this profile without user interaction
                    LoadProfileList(profileToLoad);
                }

                StartProfile();
            }
        }

        public void StartProfile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void StartProfile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_deletingProfile)
            {
                if (_profileIndex < _profiles.Count)
                {
                    File.Delete(_profiles[_profileIndex]);
                    _profiles.RemoveAt(_profileIndex);
                    if (_profileIndex == _profiles.Count)
                    {
                        _profileIndex--;
                        if (_profileIndex > -1)
                        {
                            SelectedProfileName = System.IO.Path.GetFileNameWithoutExtension(_profiles[_profileIndex]);
                        }
                        else
                        {
                            SelectedProfileName = "- No Profiles Available -";
                        }
                        ActiveProfile = null;
                    }
                    LoadProfileList();
                }
            }
            else
            {
                if (ActiveProfile != null)
                {
                    if (ActiveProfile.IsStarted)
                    {
                        return;
                    }

                    if (File.GetLastWriteTime(ActiveProfile.Path) > ActiveProfile.LoadTime)
                    {
                        LoadProfile(ActiveProfile.Path);
                    }
                }
                else if (_profileIndex >= 0)
                {
                    LoadProfile(_profiles[_profileIndex]);
                }

                StartProfile();
            }
        }

        public void StopProfile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void StopProfile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _deletingProfile = false;
            StopProfile();
            StatusMessage = StatusValue.License;
        }

        public void ResetProfile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ResetProfile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_deletingProfile)
            {
                _deletingProfile = false;
                StatusMessage = StatusValue.License;
            }
            ResetProfile();
        }

        private void OpenControlCenter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Maximize();
        }

        private void TogglePreferences_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _prefsShown = !_prefsShown;
            if (_prefsShown)
            {
                PreferencesCanvas.Visibility = System.Windows.Visibility.Visible;
                Height = 277 + 320;
            }
            else
            {
                PreferencesCanvas.Visibility = System.Windows.Visibility.Collapsed;
                Height = 277;
            }
        }

        private void PrevProfile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _deletingProfile = false;
            StatusMessage = StatusValue.License;

            LoadProfileList();

            if (_profileIndex > 0 && _profiles.Count > 0)
            {
                if (ActiveProfile != null && ActiveProfile.IsStarted)
                {
                    StopProfile();
                }

                ActiveProfile = null;
                _profileIndex--;
                SelectedProfileName = System.IO.Path.GetFileNameWithoutExtension(_profiles[_profileIndex]);
            }
        }

        private void NextProfile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _deletingProfile = false;
            StatusMessage = StatusValue.License;

            LoadProfileList();

            if (_profileIndex < _profiles.Count - 1)
            {
                if (ActiveProfile != null && ActiveProfile.IsStarted)
                {
                    StopProfile();
                }

                ActiveProfile = null;
                _profileIndex++;
                SelectedProfileName = System.IO.Path.GetFileNameWithoutExtension(_profiles[_profileIndex]);
            }
        }

        private void DeleteProfile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _deletingProfile = true;
            StatusMessage = StatusValue.DeleteWarning;
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Profile Running

        private void StartProfile()
        {
            if (ActiveProfile != null && !ActiveProfile.IsStarted)
            {
                ActiveProfile.ControlCenterShown += Profile_ShowControlCenter;
                ActiveProfile.ControlCenterHidden += Profile_HideControlCenter;
                ActiveProfile.ProfileStopped += Profile_ProfileStopped;
                ActiveProfile.ProfileHintReceived += Profile_ProfileHintReceived;
                ActiveProfile.DriverStatusReceived += Profile_DriverStatusReceived;
                ActiveProfile.ClientChanged += Profile_ClientChanged;

                ActiveProfile.Dispatcher = Dispatcher;
                ActiveProfile.Start();

                if (_dispatcherTimer != null)
                {
                    _dispatcherTimer.Stop();
                }

                _dispatcherTimer = new DispatcherTimer();
                _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 33);
                _dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
                _dispatcherTimer.Start();

                foreach (Monitor monitor in ActiveProfile.Monitors)
                {
                    try
                    {
                        if (monitor.Children.Count > 0 || monitor.FillBackground || !String.IsNullOrWhiteSpace(monitor.BackgroundImage))
                        {
                            if (ConfigManager.SettingsManager.IsSettingAvailable("ControlCenter", "TouchScreenMouseSuppressionPeriod"))
                            {
                                monitor.SuppressMouseAfterTouchDuration = ConfigManager.SettingsManager.LoadSetting("ControlCenter", "TouchScreenMouseSuppressionPeriod", 0);
                            }
                            else
                            {
                                monitor.SuppressMouseAfterTouchDuration = 0;
                            }
                            ConfigManager.LogManager.LogDebug("Creating window (Monitor=\"" + monitor.Name + "\")" + " with Touchscreen 2nd Trigger suppression delay set to " + Convert.ToString(monitor.SuppressMouseAfterTouchDuration) + " msec.");
                            MonitorWindow window = new MonitorWindow(monitor, true);
                            window.Show();
                            _windows.Add(window);
                        }
                    }
                    catch (Exception ex)
                    {
                        ConfigManager.LogManager.LogError("Error creating monitor window (Monitor=\"" + monitor.Name + "\")", ex);
                    }
                }

                //App app = Application.Current as App;
                //if (app == null || (app != null && !app.DisableTouchKit))
                //{
                //    try
                //    {
                //        EGalaxTouch.CaptureTouchScreens(_windows);
                //    }
                //    catch (Exception ex)
                //    {
                //        ConfigManager.LogManager.LogError("Error capturing touchkit screens.", ex);
                //    }
                //}

                // success, consider this the most recently run profile for its tags (usually just the aircraft types supported)
                foreach (string tag in ActiveProfile.Tags)
                {
                    ConfigManager.SettingsManager.SaveSetting("RecentByTag", tag, ActiveProfile.Path);
                }

                StatusMessage = StatusValue.Running;

                if (AutoHideCheckBox.IsChecked == true)
                {
                    Minimize();
                }
                else
                {
                    NativeMethods.BringWindowToTop(_helper.Handle);
                }
            }

        }

        private void Profile_DriverStatusReceived(object sender, DriverStatus e)
        {
            _lastDriverStatus = e.ExportDriver;
            UpdateStatusMessage();
            ConfigManager.LogManager.LogDebug($"received profile status indicating that simulator is running exports for '{e.ExportDriver}'");
        }

        private void Profile_ProfileHintReceived(object sender, ProfileHint e)
        {
            _lastProfileHint = e.Tag;
            UpdateStatusMessage();
            if (ProfileAutoStartCheckBox.IsChecked == false)
            {
                return;
            }
            ConfigManager.LogManager.LogDebug($"received profile hint with tag '{e.Tag}'");
            string mostRecent = ConfigManager.SettingsManager.LoadSetting("RecentByTag", e.Tag, null);
            if (mostRecent == null)
            {
                ConfigManager.LogManager.LogInfo($"received profile hint with tag '{e.Tag}' but no matching profile has been loaded; cannot auto load");
                return;
            }
            if ((ActiveProfile != null) && (ActiveProfile.Path == mostRecent))
            {
                ConfigManager.LogManager.LogDebug($"most recent profile for profile hint with tag '{e.Tag}' is already active");
                // ask simulator to use the one we are running, if possible
                ActiveProfile.RequestProfileSupport();
                return;
            }
            // execute auto load
            ConfigManager.LogManager.LogDebug($"trying to start most recent matching profile '{mostRecent}'");
            ControlCenterCommands.RunProfile.Execute(mostRecent, Application.Current.MainWindow);
        }

        private void Profile_ClientChanged(object sender, ProfileAwareInterface.ClientChange e)
        {
            if (e.FromOpaqueHandle != ProfileAwareInterface.ClientChange.NO_CLIENT)
            {
                // this is a change during our profile's lifetime, so we need to discard information we have
                _lastProfileHint = "";
            }
            _lastDriverStatus = "";
        }

        private void StopProfile()
        {
            if (ActiveProfile != null && ActiveProfile.IsStarted)
            {
                ActiveProfile.Stop();
            }
        }

        private void ResetProfile()
        {
            if (ActiveProfile != null)
            {
                ActiveProfile.Reset();
            }
        }

        private void Profile_ShowControlCenter(object sender, EventArgs e)
        {
            Maximize();
        }

        private void Profile_HideControlCenter(object sender, EventArgs e)
        {
            Minimize();
        }

        void Profile_ProfileStopped(object sender, EventArgs e)
        {
            foreach (MonitorWindow window in _windows)
            {
                window.Close();
            }

            _windows.Clear();

            HeliosProfile profile = sender as HeliosProfile;
            if (profile != null)
            {
                profile.ControlCenterShown -= Profile_ShowControlCenter;
                profile.ControlCenterHidden -= Profile_HideControlCenter;
                profile.ProfileStopped -= Profile_ProfileStopped;
                profile.ProfileHintReceived -= Profile_ProfileHintReceived;
                profile.DriverStatusReceived -= Profile_DriverStatusReceived;
                profile.ClientChanged -= Profile_ClientChanged;
            }

            if (_dispatcherTimer != null)
            {
                _dispatcherTimer.Stop();
                _dispatcherTimer = null;
            }

            StatusMessage = StatusValue.License;

            //EGalaxTouch.ReleaseTouchScreens();
        }

        void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (ActiveProfile != null)
            {
                try
                {
                    ActiveProfile.Tick();
                    long currentTick = DateTime.Now.Ticks;
                    if (currentTick - _lastTick > TOPMOST_TICK_COUNT)
                    {
                        for (int i = 0; i < _windows.Count; i++)
                        {
                            if (_windows[i].Monitor.AlwaysOnTop)
                            {
                                NativeMethods.SetWindowPos(_windows[i].Handle, HWND_TOPMOST, 0, 0, 0, 0, NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);
                            }
                        }
                        NativeMethods.SetWindowPos(_helper.Handle, HWND_TOPMOST, 0, 0, 0, 0, NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);
                        _lastTick = currentTick;
                    }
                }
                catch (Exception exception)
                {
                    ConfigManager.LogManager.LogError("Error processing profile tick or refresh.", exception);
                }
            }
        }

        #endregion

        #region Profile Persistance

        private void LoadProfile(string path)
        {
            if (ActiveProfile == null || (ActiveProfile != null && ActiveProfile.LoadTime < Directory.GetLastWriteTime(ActiveProfile.Path)))
            {
                StatusMessage = StatusValue.Loading;

                // pump main thread to update UI
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Loaded, (Action)delegate { });

                // now do the load that might take a while
                ActiveProfile = ConfigManager.ProfileManager.LoadProfile(path, Dispatcher);
            }

            if (ActiveProfile != null)
            {
                NativeMethods.SHAddToRecentDocs(0x00000003, path);

                SelectedProfileName = System.IO.Path.GetFileNameWithoutExtension(ActiveProfile.Path);
#if !DEBUG
                if (!ActiveProfile.IsValidMonitorLayout)
                {
                    StatusMessage = StatusValue.BadMonitorConfig;
                    ActiveProfile = null;
                    return;
                }
#endif

                if (ActiveProfile.IsInvalidVersion)
                {
                    StatusMessage = StatusValue.ProfileVersionHigher;
                    ActiveProfile = null;
                    return;
                }
            }
            else
            {
                StatusMessage = StatusValue.LoadError;
            }
        }

        private void LoadProfileList()
        {
            string currentProfilePath = "";
            if (_profileIndex >= 0 && _profileIndex < _profiles.Count)
            {
                currentProfilePath = _profiles[_profileIndex];
            }
            LoadProfileList(currentProfilePath);
        }

        /// <summary>
        /// reload the list of profiles on disk and set the current selected index if
        /// the given path matches references a file that still exists
        /// </summary>
        /// <param name="currentProfilePath"></param>
        private void LoadProfileList(string currentProfilePath)
        {
            _profileIndex = -1;
            _profiles.Clear();

            foreach (string file in Directory.GetFiles(ConfigManager.ProfilePath, "*.hpf"))
            {
                if (currentProfilePath != null && file.Equals(currentProfilePath))
                {
                    _profileIndex = _profiles.Count;
                    SelectedProfileName = System.IO.Path.GetFileNameWithoutExtension(file);
                }
                _profiles.Add(file);
            }
        }

        #endregion

        #region Windows Event Handlers

        private void MoveThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Left = Left + e.HorizontalChange;
            Top = Top + e.VerticalChange;
        }

        private void TouchScreenDelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs < double > e)
        {
            Int16 delayValue = Convert.ToInt16(e.NewValue);
            string msg = String.Format(" {0} ms", delayValue);
            this.TouchscreenDelayTextBlock.Text = msg;
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "TouchScreenMouseSuppressionPeriod", Convert.ToString(delayValue));
        }
        private void PowerButton_Unchecked(object sender, RoutedEventArgs e)
        {
            _deletingProfile = false;
            DispatcherTimer minimizeTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 250), DispatcherPriority.Normal, TimedMinimize, Dispatcher);
        }

        void TimedMinimize(object sender, EventArgs e)
        {
            (sender as DispatcherTimer).Stop();
            Dispatcher.Invoke(new Action(Close), null);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (ActiveProfile != null && ActiveProfile.IsStarted)
            {
                ActiveProfile.Stop();
            }

            ConfigManager.LogManager.LogInfo("Saving control center window position.");

            // Persist window placement details to application settings
            WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            NativeMethods.GetWindowPlacement(hwnd, out wp);

            //Properties.Settings.Default.ControlCenterPlacement = wp;
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "WindowLocation", wp.normalPosition);

            if (ActiveProfile != null && _profileIndex >= 0 && _profileIndex < _profiles.Count)
            {
                ConfigManager.SettingsManager.SaveSetting("ControlCenter", "LastProfile", _profiles[_profileIndex]);
            }

            Properties.Settings.Default.Save();

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (_hotkey != null)
            {
                _hotkey.UnregisterHotKey();
                _hotkey.Dispose();
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            try
            {
                // Load window placement details for previous application session from application settings
                // Note - if window was closed on a monitor that is now disconnected from the computer,
                //        SetWindowPlacement will place the window onto a visible monitor.

                if (ConfigManager.SettingsManager.IsSettingAvailable("ControlCenter", "WindowLocation"))
                {
                    WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
                    wp.normalPosition = ConfigManager.SettingsManager.LoadSetting("ControlCenter", "WindowLocation", new RECT(0, 0, (int)Width, (int)Height));
                    wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                    wp.flags = 0;
                    wp.showCmd = (wp.showCmd == NativeMethods.SW_SHOWMINIMIZED ? NativeMethods.SW_SHOWNORMAL : wp.showCmd);
                    IntPtr hwnd = new WindowInteropHelper(this).Handle;
                    NativeMethods.SetWindowPlacement(hwnd, ref wp);
                }

                ModifierKeys mods = (ModifierKeys)Enum.Parse(typeof(ModifierKeys), ConfigManager.SettingsManager.LoadSetting("ControlCenter", "HotKeyModifiers", "None"));
                Keys hotKey = (Keys)Enum.Parse(typeof(Keys), ConfigManager.SettingsManager.LoadSetting("ControlCenter", "HotKey", "None"));
                if (hotKey != Keys.None)
                {
                    _hotkey = new HotKey(mods, hotKey, this);
                    _hotkey.HotKeyPressed += new Action<HotKey>(HotKeyPressed);
                    HotKeyDescription = KeyboardEmulator.ModifierKeysToString(_hotkey.KeyModifier) + _hotkey.Key.ToString();
                }
                else
                {
                    HotKeyDescription = "None";
                }
            }
            catch { }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Set the window style to noactivate.
            _helper = new WindowInteropHelper(this);

            NativeMethods.SetWindowLong(_helper.Handle,
                NativeMethods.GWL_EXSTYLE,
                NativeMethods.GetWindowLong(_helper.Handle, NativeMethods.GWL_EXSTYLE) | NativeMethods.WS_EX_NOACTIVATE);
        }

        private void Window_Opened(object sender, EventArgs e)
        {
            Height = _prefsShown ? 277+320 : 277;
            Width = 504;

            if (Environment.OSVersion.Version.Major > 5 && ConfigManager.SettingsManager.LoadSetting("ControlCenter", "AeroWarning", true))
            {
                bool aeroEnabled;
                NativeMethods.DwmIsCompositionEnabled(out aeroEnabled);
                if (!aeroEnabled)
                {
                    AeroWarning warningDialog = new AeroWarning();
                    warningDialog.Owner = this;
                    warningDialog.ShowDialog();

                    if (warningDialog.DisplayAgainCheckbox.IsChecked == true)
                    {
                        ConfigManager.SettingsManager.SaveSetting("ControlCenter", "AeroWarning", false);
                    }
                }
            }


            App app = Application.Current as App;
            if (app != null && app.StartupProfile != null && File.Exists(app.StartupProfile))
            {
                LoadProfileList(app.StartupProfile);
                LoadProfile(app.StartupProfile);
                StartProfile();
            }

            VersionChecker.CheckVersion();

        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        #endregion

        #region Preferences

        private void AutoStartCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RegistryKey pathKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            pathKey.SetValue("Helios", "\"" + System.IO.Path.Combine(ConfigManager.ApplicationPath, "ControlCenter.exe") + "\"");
            pathKey.Close();
        }

        private void MinimizeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "StartMinimized", true);
        }

        private void AutoStartCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            RegistryKey pathKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            pathKey.DeleteValue("Helios", false);
            pathKey.Close();
        }

        private void ProfileAutoStartCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "ProfileAutoStart", true);
        }

        private void ProfileAutoStartCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "ProfileAutoStart", false);
        }

        private void MinimizeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "StartMinimized", false);
        }

        private void AutoHideCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "AutoHide", true);
        }

        private void AutoHideCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "AutoHide", false);
        }

        private void TouchscreenCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //ConfigManager.SettingsManager.SaveSetting("ControlCenter", "AutoHide", true);
            TouchscreenDelayTextBlock.Visibility = Visibility.Visible;
            TouchScreenDelaySlider.Visibility = Visibility.Visible;
            TouchScreenDelayBorder.Visibility = Visibility.Visible;
            TouchScreenDelayTitle.Visibility = Visibility.Visible;
        }

        private void TouchscreenCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "TouchScreenMouseSuppressionPeriod", "0");
            TouchScreenDelaySlider.Value = 0;
            TouchscreenDelayTextBlock.Visibility = Visibility.Hidden;
            TouchScreenDelaySlider.Visibility = Visibility.Hidden;
            TouchScreenDelayBorder.Visibility = Visibility.Hidden;
            TouchScreenDelayTitle.Visibility = Visibility.Hidden;
        }
        private void TBDCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //ConfigManager.SettingsManager.SaveSetting("ControlCenter", "AutoHide", true);
        }

        private void TBDCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //ConfigManager.SettingsManager.SaveSetting("ControlCenter", "AutoHide", false);
         }
        private void SetHotkey_Click(object sender, RoutedEventArgs e)
        {
            HotKeyDetector detector = new HotKeyDetector();
            detector.Owner = this;
            detector.ShowDialog();

            if (_hotkey != null)
            {
                _hotkey.UnregisterHotKey();
                _hotkey.Dispose();
            }

            _hotkey = new HotKey(detector.Modifiers, detector.Key, _helper.Handle);
            _hotkey.HotKeyPressed += new Action<HotKey>(HotKeyPressed);

            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "HotKeyModifiers", detector.Modifiers.ToString());
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "HotKey", detector.Key.ToString());

            HotKeyDescription = KeyboardEmulator.ModifierKeysToString(_hotkey.KeyModifier) + _hotkey.Key.ToString();
        }

        private void ClearHotkey_Click(object sender, RoutedEventArgs e)
        {
            if (_hotkey != null)
            {
                _hotkey.UnregisterHotKey();
                _hotkey.Dispose();
            }

            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "HotKeyModifiers", ModifierKeys.None.ToString());
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "HotKey", Keys.None.ToString());

            HotKeyDescription = "None";
        }

        #endregion

        #region Hotkey Handling

        void HotKeyPressed(HotKey obj)
        {
            Maximize();
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Minimize();
        }
    }
}
